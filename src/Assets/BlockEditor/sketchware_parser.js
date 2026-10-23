// Visual Block JSON to Blockly Converter
// This allows SnapCode to dynamically load custom blocks from block.json and palette.json

function parseSketchwareBlocks(paletteJson, blockJson) {
    try {
        const palettes = typeof paletteJson === 'string' ? JSON.parse(paletteJson) : paletteJson;
        const blocks = typeof blockJson === 'string' ? JSON.parse(blockJson) : blockJson;
        
        const blocklyBlocks = [];
        const toolboxCategories = {};
        
        // Initialize toolbox categories
        palettes.forEach((pal, index) => {
            toolboxCategories[index] = {
                name: pal.name,
                color: pal.color,
                blocks: []
            };
        });

        blocks.forEach(skBlock => {
            const blockName = "sk_" + skBlock.name;
            
            // 1. Determine Block Shape & Returns based on block 'type'
            const def = {
                type: blockName,
                colour: skBlock.color || (toolboxCategories[skBlock.palette] ? toolboxCategories[skBlock.palette].color : '#999999'),
                tooltip: skBlock.spec
            };

            const t = skBlock.type;
            if (t === " ") { // Regular command
                def.previousStatement = null;
                def.nextStatement = null;
            } else if (t === "c") { // C-shape (wrap)
                def.previousStatement = null;
                def.nextStatement = null;
            } else if (t === "e") { // E-shape (if/else)
                def.previousStatement = null;
                def.nextStatement = null;
            } else if (t === "s") { // String return
                def.output = "String";
            } else if (t === "d") { // Number return
                def.output = "Number";
            } else if (t === "b") { // Boolean return
                def.output = "Boolean";
            } else if (t === "f") { // Final / Cap
                def.previousStatement = null;
            }

            // 2. Parse Spec to Blockly message and args
            let specString = skBlock.spec;
            let args0 = [];
            let messageParts = [];
            
            // Match block parameters like %s, %d, %b, %m.view, %s.inputOnly
            const regex = /(%s\.inputOnly|%m\.[a-zA-Z0-9_]+|%s|%d|%b)/g;
            let paramIndex = 1;
            let lastIdx = 0;
            
            let match;
            while ((match = regex.exec(specString)) !== null) {
                // Add static text before the parameter
                const staticText = specString.substring(lastIdx, match.index).trim();
                if (staticText) {
                    messageParts.push(staticText);
                }
                
                // Add parameter
                const paramStr = match[0];
                messageParts.push(`%${paramIndex}`);
                
                if (paramStr.startsWith("%m.")) {
                    // Dropdown
                    args0.push({
                        type: "field_dropdown",
                        name: `PARAM${paramIndex}`,
                        options: [["(select)", ""]]
                    });
                } else if (paramStr === "%s.inputOnly") {
                    args0.push({
                        type: "field_input",
                        name: `PARAM${paramIndex}`,
                        text: ""
                    });
                } else if (paramStr === "%s" || paramStr === "%d" || paramStr === "%b") {
                    args0.push({
                        type: "input_value",
                        name: `PARAM${paramIndex}`
                    });
                }
                
                paramIndex++;
                lastIdx = regex.lastIndex;
            }
            
            const trailingText = specString.substring(lastIdx).trim();
            if (trailingText) {
                messageParts.push(trailingText);
            }
            
            def.message0 = messageParts.join(" ");
            if (args0.length > 0) {
                def.args0 = args0;
            }

            // Add C-shape / E-shape statement inputs
            if (t === "c") {
                def[`message${args0.length > 0 ? 1 : 0}`] = "%1";
                if (!def[`args${args0.length > 0 ? 1 : 0}`]) def[`args${args0.length > 0 ? 1 : 0}`] = [];
                def[`args${args0.length > 0 ? 1 : 0}`].push({ type: "input_statement", name: "DO" });
            } else if (t === "e") {
                def[`message${args0.length > 0 ? 1 : 0}`] = "%1";
                if (!def[`args${args0.length > 0 ? 1 : 0}`]) def[`args${args0.length > 0 ? 1 : 0}`] = [];
                def[`args${args0.length > 0 ? 1 : 0}`].push({ type: "input_statement", name: "DO_IF" });
                
                const nextIdx = args0.length > 0 ? 2 : 1;
                def[`message${nextIdx}`] = (skBlock.spec2 || "else") + " %1";
                def[`args${nextIdx}`] = [{ type: "input_statement", name: "DO_ELSE" }];
            }

            blocklyBlocks.push(def);
            
            // Add to toolbox category
            if (toolboxCategories[skBlock.palette]) {
                toolboxCategories[skBlock.palette].blocks.push(blockName);
            }

            // 3. Register Dart Code Generator
            dartGenerator.forBlock[blockName] = function(block) {
                let code = skBlock.code || "";
                
                // Replace %1$s, %2$s with actual values
                for (let i = 1; i < paramIndex; i++) {
                    const arg = args0[i-1];
                    let val = "";
                    if (arg.type === "field_dropdown" || arg.type === "field_input") {
                        val = block.getFieldValue(`PARAM${i}`) || "";
                    } else if (arg.type === "input_value") {
                        val = dartGenerator.valueToCode(block, `PARAM${i}`, dartGenerator.ORDER_ATOMIC) || "";
                    }
                    code = code.replace(new RegExp(`%${i}\\$s`, 'g'), val);
                    code = code.replace(new RegExp(`%${i}\\$d`, 'g'), val);
                    code = code.replace(new RegExp(`%${i}\\$b`, 'g'), val);
                }
                
                if (t === "c") {
                    const innerCode = dartGenerator.statementToCode(block, 'DO');
                    code = code.replace("%c$s", innerCode);
                } else if (t === "e") {
                    const ifCode = dartGenerator.statementToCode(block, 'DO_IF');
                    const elseCode = dartGenerator.statementToCode(block, 'DO_ELSE');
                    code = code + `\n${ifCode}\nelse {\n${elseCode}\n}`; 
                }

                if (t === "s" || t === "d" || t === "b") {
                    return [code, dartGenerator.ORDER_NONE];
                } else {
                    return code + (code.trim().endsWith(";") || code.trim().endsWith("}") || code.trim() === "" ? "\n" : ";\n");
                }
            };
        });

        // Load blocks into Blockly
        Blockly.defineBlocksWithJsonArray(blocklyBlocks);
        
        // Inject into Toolbox XML
        const toolbox = document.getElementById('toolbox');
        if (toolbox) {
            Object.values(toolboxCategories).forEach(cat => {
                if (cat.blocks.length === 0) return; // Skip empty categories
                
                const categoryNode = document.createElement('category');
                categoryNode.setAttribute('name', "SW: " + cat.name);
                if (cat.color) categoryNode.setAttribute('colour', cat.color);

                cat.blocks.forEach(blockType => {
                    const blockNode = document.createElement('block');
                    blockNode.setAttribute('type', blockType);
                    categoryNode.appendChild(blockNode);
                });

                toolbox.appendChild(categoryNode);
                
                // Keep the global clones in sync if they exist
                if (window.originalToolbox) {
                    window.originalToolbox.appendChild(categoryNode.cloneNode(true));
                }
                if (window.currentToolbox) {
                    window.currentToolbox.appendChild(categoryNode.cloneNode(true));
                }
            });

            if (typeof workspace !== 'undefined' && workspace !== null) {
                if (window.currentToolbox) {
                    workspace.updateToolbox(window.currentToolbox);
                } else {
                    workspace.updateToolbox(toolbox);
                }
            }
        }
        
        return toolboxCategories;
    } catch (e) {
        console.error("Sketchware Parser Error:", e);
    }
}
