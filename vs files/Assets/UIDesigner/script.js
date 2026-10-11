document.addEventListener('DOMContentLoaded', () => {
    const paletteItems = document.querySelectorAll('.widget-item');
    const phoneCanvas = document.querySelector('.phone-canvas');

    // 1. Make palette items draggable
    paletteItems.forEach(item => {
        item.setAttribute('draggable', 'true');
        item.addEventListener('dragstart', (e) => {
            e.dataTransfer.setData('text/plain', item.dataset.type);
            e.dataTransfer.effectAllowed = 'copy';
        });
    });

    // 2 & 7. Helper to make an element a valid drop target handling drag events
    function setupDropTarget(element) {
        element.addEventListener('dragover', (e) => {
            e.preventDefault(); // Necessary to allow dropping
            e.dataTransfer.dropEffect = 'copy';
            e.stopPropagation(); // Avoid triggering parent drop targets
        });

        element.addEventListener('dragenter', (e) => {
            e.preventDefault();
            element.classList.add('drag-over');
            e.stopPropagation();
        });

        element.addEventListener('dragleave', (e) => {
            element.classList.remove('drag-over');
            e.stopPropagation();
        });

        element.addEventListener('drop', (e) => {
            e.preventDefault();
            e.stopPropagation();
            element.classList.remove('drag-over');

            const widgetType = e.dataTransfer.getData('text/plain');
            if (widgetType) {
                createWidget(widgetType, element);
            }
        });
    }

    // 2. Set up phone canvas as initial drop target
    if (phoneCanvas) {
        setupDropTarget(phoneCanvas);

        // Click on empty canvas to deselect widgets
        phoneCanvas.addEventListener('click', (e) => {
            if (e.target === phoneCanvas) {
                clearSelection();
            }
        });
    }

    // 3. Create DOM element when dropped
    function createWidget(type, parent) {
        const widget = document.createElement('div');
        widget.classList.add('widget', `widget-${type.toLowerCase()}`);
        widget.dataset.type = type;

        const isContainer = ['Row', 'Column', 'Container'].includes(type);

        if (!isContainer) {
            // Give non-container widgets some text so they are visible
            widget.textContent = type;
        }

        // 6. Handle selection and stop propagation
        widget.addEventListener('click', (e) => {
            e.stopPropagation(); // Only select the deepest widget
            clearSelection();
            widget.classList.add('selected');
        });

        // Make container widgets valid drop targets
        if (isContainer) {
            widget.classList.add('container-widget');
            setupDropTarget(widget);
        }

        parent.appendChild(widget);
    }

    function clearSelection() {
        document.querySelectorAll('.widget.selected').forEach(el => {
            el.classList.remove('selected');
        });
    }
});

// 4 & 5. Build the JSON tree representation
window.getUITree = function() {
    const phoneCanvas = document.querySelector('.phone-canvas');
    if (!phoneCanvas) return { type: 'Root', children: [] };
    
    function buildTree(element) {
        const node = {
            type: element.dataset.type,
            children: []
        };

        const children = Array.from(element.children).filter(child => child.classList.contains('widget'));
        for (const child of children) {
            node.children.push(buildTree(child));
        }

        return node;
    }

    const root = { type: 'Root', children: [] };
    const rootWidgets = Array.from(phoneCanvas.children).filter(child => child.classList.contains('widget'));
    for (const child of rootWidgets) {
        root.children.push(buildTree(child));
    }

    return root;
};
