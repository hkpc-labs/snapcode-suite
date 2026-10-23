using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Snapcode.Models;
using SnapCode.Services;

namespace Snapcode.Services
{
    /// <summary>
    /// Service to manage and instantiate pre-built project templates
    /// complete with visual View Designer components and Blockly logic blocks.
    /// </summary>
    public static class TemplateService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Creates a complete Tic-Tac-Toe project with View Designer widgets,
        /// Blockly event blocks, state variables, and compiled Flutter code.
        /// </summary>
        public static async Task CreateTicTacToeProjectAsync(string projectRoot, string projectName)
        {
            Directory.CreateDirectory(projectRoot);

            // 1. Initialize .snapcode directory structure
            string metaPath = ProjectService.GetMetaPath(projectRoot);
            string pagesDir = ProjectService.GetPagesDirectory(projectRoot);
            Directory.CreateDirectory(metaPath);
            Directory.CreateDirectory(pagesDir);
            Directory.CreateDirectory(Path.Combine(metaPath, "assets"));
            Directory.CreateDirectory(Path.Combine(metaPath, "logs"));

            try
            {
                var dirInfo = new DirectoryInfo(metaPath);
                dirInfo.Attributes |= FileAttributes.Hidden;
            }
            catch { }

            // 2. Write project.json
            var project = new SnapcodeProject
            {
                Name = projectName,
                Organization = "com.example",
                Platforms = "windows,web,android",
                UseKotlin = true,
                CreatedAt = DateTime.UtcNow,
                LastOpenedAt = DateTime.UtcNow
            };
            await File.WriteAllTextAsync(Path.Combine(metaPath, "project.json"), JsonSerializer.Serialize(project, JsonOptions));

            // 3. Write pubspec.yaml
            string flutterDir = Path.Combine(projectRoot, ProjectService.FlutterFolderName);
            string flutterLibDir = Path.Combine(flutterDir, "lib");
            string legacyLibDir = Path.Combine(projectRoot, "lib");
            Directory.CreateDirectory(flutterLibDir);
            Directory.CreateDirectory(legacyLibDir);

            string pubspecYaml = $@"name: {projectName}
description: ""A Tic-Tac-Toe game created with Snapcode""
publish_to: 'none'
version: 1.0.0+1

environment:
  sdk: '>=3.0.0 <4.0.0'

dependencies:
  flutter:
    sdk: flutter

dev_dependencies:
  flutter_test:
    sdk: flutter

flutter:
  uses-material-design: true
";
            await File.WriteAllTextAsync(Path.Combine(flutterDir, "pubspec.yaml"), pubspecYaml);
            await File.WriteAllTextAsync(Path.Combine(projectRoot, "pubspec.yaml"), pubspecYaml);

            string analysisYaml = @"analyzer:
  exclude:
    - build/**
    - android/**
    - ios/**
    - web/**
    - windows/**
";
            await File.WriteAllTextAsync(Path.Combine(flutterDir, "analysis_options.yaml"), analysisYaml);

            // 4. Generate Page Metadata (.snapcode/pages/main_meta.json)
            var meta = new PageMetadata
            {
                Name = "main",
                Title = "Tic-Tac-Toe Game",
                Type = "Page",
                ShowAppBar = true,
                AppBarTitle = "Tic-Tac-Toe Game",
                AppBarBackgroundColor = "#1E293B",
                AppBarForegroundColor = "#38BDF8",
                AppBarElevation = 3.0,
                AppBarCenterTitle = true,
                AppBarLeadingIcon = "none",
                ScaffoldBackgroundColor = "#0F172A",
                SafeArea = true,
                Orientation = "Portrait",
                HasFAB = false,
                HasDrawer = false
            };
            await File.WriteAllTextAsync(Path.Combine(pagesDir, "main_meta.json"), JsonSerializer.Serialize(meta, JsonOptions));

            // 5. Generate View Designer UI Schema (.snapcode/pages/main_ui.json)
            string uiJson = GetTicTacToeUiJson();
            await File.WriteAllTextAsync(Path.Combine(pagesDir, "main_ui.json"), uiJson);

            // 6. Generate Event Workspaces and Dart Event Code for all 9 cells + reset button
            var allOverviewBlocks = new List<object>();
            int blockX = 40;
            int blockY = 40;

            // 6a. Generate cell buttons
            for (int i = 0; i < 9; i++)
            {
                string cellId = $"btn_cell_{i}";
                int row = i / 3;
                int col = i % 3;
                int posX = 40 + (col * 360);
                int posY = 40 + (row * 300);

                var (cellWorkspaceJson, cellBlockObj) = GetCellClickBlocklyWorkspace(cellId, i, posX, posY);
                await File.WriteAllTextAsync(Path.Combine(pagesDir, $"main_{cellId}_onClick.json"), cellWorkspaceJson);

                string cellEventDart = GetCellClickEventDart(cellId, i);
                await File.WriteAllTextAsync(Path.Combine(pagesDir, $"main_{cellId}_onClick.event.dart"), cellEventDart);

                allOverviewBlocks.Add(cellBlockObj);
            }

            // 6b. Generate reset button
            var (resetWorkspaceJson, resetBlockObj) = GetResetClickBlocklyWorkspace(40, 960);
            await File.WriteAllTextAsync(Path.Combine(pagesDir, "main_btn_reset_onClick.json"), resetWorkspaceJson);

            string resetEventDart = @"// EVENT_BINDING: onClick -> btn_reset
setState(() {
  _txtStatusText = ""Player X's Turn"";
});
HapticFeedback.selectionClick();
_restartGame();
";
            await File.WriteAllTextAsync(Path.Combine(pagesDir, "main_btn_reset_onClick.event.dart"), resetEventDart);
            allOverviewBlocks.Add(resetBlockObj);

            // 6c. Add visual Blockly blocks for state variables and core game methods to the Overview workspace
            var stateVarsBlockObj = new
            {
                type = "declare_state_variable",
                id = "block_var_board",
                x = 1140,
                y = 40,
                fields = new Dictionary<string, object>
                {
                    ["TYPE"] = "List<String>",
                    ["NAME"] = "_board",
                    ["VALUE"] = "List.filled(9, '')"
                },
                next = new
                {
                    block = new
                    {
                        type = "declare_state_variable",
                        id = "block_var_player",
                        fields = new Dictionary<string, object>
                        {
                            ["TYPE"] = "String",
                            ["NAME"] = "_currentPlayer",
                            ["VALUE"] = "'X'"
                        },
                        next = new
                        {
                            block = new
                            {
                                type = "declare_state_variable",
                                id = "block_var_gameover",
                                fields = new Dictionary<string, object>
                                {
                                    ["TYPE"] = "bool",
                                    ["NAME"] = "_gameOver",
                                    ["VALUE"] = "false"
                                },
                                next = new
                                {
                                    block = new
                                    {
                                        type = "declare_state_variable",
                                        id = "block_var_scorex",
                                        fields = new Dictionary<string, object>
                                        {
                                            ["TYPE"] = "int",
                                            ["NAME"] = "_scoreX",
                                            ["VALUE"] = "0"
                                        },
                                        next = new
                                        {
                                            block = new
                                            {
                                                type = "declare_state_variable",
                                                id = "block_var_scoreo",
                                                fields = new Dictionary<string, object>
                                                {
                                                    ["TYPE"] = "int",
                                                    ["NAME"] = "_scoreO",
                                                    ["VALUE"] = "0"
                                                },
                                                next = new
                                                {
                                                    block = new
                                                    {
                                                        type = "declare_state_variable",
                                                        id = "block_var_scoreties",
                                                        fields = new Dictionary<string, object>
                                                        {
                                                            ["TYPE"] = "int",
                                                            ["NAME"] = "_scoreTies",
                                                            ["VALUE"] = "0"
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            allOverviewBlocks.Add(stateVarsBlockObj);

            var makeMoveBlockObj = new
            {
                type = "logic_function",
                id = "block_func_makeMove",
                x = 1140,
                y = 220,
                fields = new Dictionary<string, object>
                {
                    ["FUNC_NAME"] = "_makeMove(int index, BuildContext context)"
                },
                inputs = new Dictionary<string, object>
                {
                    ["BODY"] = new
                    {
                        block = new
                        {
                            type = "logic_if_else",
                            id = "if_valid_move",
                            fields = new Dictionary<string, object>
                            {
                                ["CONDITION"] = "!_gameOver && _board[index].isEmpty"
                            },
                            inputs = new Dictionary<string, object>
                            {
                                ["DO_IF"] = new
                                {
                                    block = new
                                    {
                                        type = "list_set_index",
                                        id = "set_board_cell",
                                        fields = new Dictionary<string, object>
                                        {
                                            ["LIST_NAME"] = "_board",
                                            ["INDEX"] = "index",
                                            ["VALUE"] = "_currentPlayer"
                                        },
                                        next = new
                                        {
                                            block = new
                                            {
                                                type = "button_set_cell_text",
                                                id = "set_btn_cell",
                                                fields = new Dictionary<string, object>
                                                {
                                                    ["INDEX"] = "index",
                                                    ["TEXT"] = "_currentPlayer"
                                                },
                                                next = new
                                                {
                                                    block = new
                                                    {
                                                        type = "logic_call_function",
                                                        id = "call_check_winner",
                                                        fields = new Dictionary<string, object>
                                                        {
                                                            ["FUNC_NAME"] = "_checkWinner",
                                                            ["ARGS"] = "context"
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            allOverviewBlocks.Add(makeMoveBlockObj);

            var checkWinnerBlockObj = new
            {
                type = "logic_function",
                id = "block_func_checkWinner",
                x = 1140,
                y = 580,
                fields = new Dictionary<string, object>
                {
                    ["FUNC_NAME"] = "_checkWinner(BuildContext context)"
                },
                inputs = new Dictionary<string, object>
                {
                    ["BODY"] = new
                    {
                        block = new
                        {
                            type = "logic_for_each",
                            id = "loop_win_patterns",
                            fields = new Dictionary<string, object>
                            {
                                ["ITEM_NAME"] = "pattern",
                                ["LIST_EXPR"] = "const [[0,1,2],[3,4,5],[6,7,8],[0,3,6],[1,4,7],[2,5,8],[0,4,8],[2,4,6]]"
                            },
                            inputs = new Dictionary<string, object>
                            {
                                ["DO"] = new
                                {
                                    block = new
                                    {
                                        type = "logic_if",
                                        id = "if_win_pattern_matched",
                                        fields = new Dictionary<string, object>
                                        {
                                            ["CONDITION"] = "_board[pattern[0]].isNotEmpty && _board[pattern[0]] == _board[pattern[1]] && _board[pattern[1]] == _board[pattern[2]]"
                                        },
                                        inputs = new Dictionary<string, object>
                                        {
                                            ["DO"] = new
                                            {
                                                block = new
                                                {
                                                    type = "set_state_wrapper",
                                                    id = "setstate_win",
                                                    inputs = new Dictionary<string, object>
                                                    {
                                                        ["DO"] = new
                                                        {
                                                            block = new
                                                            {
                                                                type = "state_variable_assign",
                                                                id = "set_gameover_win",
                                                                fields = new Dictionary<string, object>
                                                                {
                                                                    ["NAME"] = "_gameOver",
                                                                    ["VALUE"] = "true"
                                                                },
                                                                next = new
                                                                {
                                                                    block = new
                                                                    {
                                                                        type = "state_variable_assign",
                                                                        id = "set_status_win",
                                                                        fields = new Dictionary<string, object>
                                                                        {
                                                                            ["NAME"] = "_txtStatusText",
                                                                            ["VALUE"] = "\"Player ${_board[pattern[0]]} Wins!\""
                                                                        },
                                                                        next = new
                                                                        {
                                                                            block = new
                                                                            {
                                                                                type = "logic_if_else",
                                                                                id = "if_winner_x",
                                                                                fields = new Dictionary<string, object>
                                                                                {
                                                                                    ["CONDITION"] = "_board[pattern[0]] == 'X'"
                                                                                },
                                                                                inputs = new Dictionary<string, object>
                                                                                {
                                                                                    ["DO_IF"] = new
                                                                                    {
                                                                                        block = new
                                                                                        {
                                                                                            type = "state_variable_assign",
                                                                                            id = "inc_score_x",
                                                                                            fields = new Dictionary<string, object>
                                                                                            {
                                                                                                ["NAME"] = "_scoreX",
                                                                                                ["VALUE"] = "_scoreX + 1"
                                                                                            },
                                                                                            next = new
                                                                                            {
                                                                                                block = new
                                                                                                {
                                                                                                    type = "state_variable_assign",
                                                                                                    id = "set_score_x_text",
                                                                                                    fields = new Dictionary<string, object>
                                                                                                    {
                                                                                                        ["NAME"] = "_txtScoreXText",
                                                                                                        ["VALUE"] = "\"Player X: $_scoreX\""
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    },
                                                                                    ["DO_ELSE"] = new
                                                                                    {
                                                                                        block = new
                                                                                        {
                                                                                            type = "state_variable_assign",
                                                                                            id = "inc_score_o",
                                                                                            fields = new Dictionary<string, object>
                                                                                            {
                                                                                                ["NAME"] = "_scoreO",
                                                                                                ["VALUE"] = "_scoreO + 1"
                                                                                            },
                                                                                            next = new
                                                                                            {
                                                                                                block = new
                                                                                                {
                                                                                                    type = "state_variable_assign",
                                                                                                    id = "set_score_o_text",
                                                                                                    fields = new Dictionary<string, object>
                                                                                                    {
                                                                                                        ["NAME"] = "_txtScoreOText",
                                                                                                        ["VALUE"] = "\"Player O: $_scoreO\""
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    },
                                                    next = new
                                                    {
                                                        block = new
                                                        {
                                                            type = "dialog_action_modal",
                                                            id = "dlg_win",
                                                            fields = new Dictionary<string, object>
                                                            {
                                                                ["TITLE"] = "🎉 Winner!",
                                                                ["MESSAGE"] = "Player has won the game!",
                                                                ["BTN_TEXT"] = "Play Again"
                                                            },
                                                            inputs = new Dictionary<string, object>
                                                            {
                                                                ["DO"] = new
                                                                {
                                                                    block = new
                                                                    {
                                                                        type = "logic_call_function",
                                                                        id = "call_restart_win",
                                                                        fields = new Dictionary<string, object>
                                                                        {
                                                                            ["FUNC_NAME"] = "_restartGame",
                                                                            ["ARGS"] = ""
                                                                        }
                                                                    }
                                                                }
                                                            },
                                                            next = new
                                                            {
                                                                block = new
                                                                {
                                                                    type = "logic_return",
                                                                    id = "ret_win",
                                                                    fields = new Dictionary<string, object>()
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            next = new
                            {
                                block = new
                                {
                                    type = "logic_if_else",
                                    id = "if_board_full_draw",
                                    fields = new Dictionary<string, object>
                                    {
                                        ["CONDITION"] = "!_board.contains('')"
                                    },
                                    inputs = new Dictionary<string, object>
                                    {
                                        ["DO_IF"] = new
                                        {
                                            block = new
                                            {
                                                type = "set_state_wrapper",
                                                id = "setstate_draw",
                                                inputs = new Dictionary<string, object>
                                                {
                                                    ["DO"] = new
                                                    {
                                                        block = new
                                                        {
                                                            type = "state_variable_assign",
                                                            id = "set_gameover_draw",
                                                            fields = new Dictionary<string, object>
                                                            {
                                                                ["NAME"] = "_gameOver",
                                                                ["VALUE"] = "true"
                                                            },
                                                            next = new
                                                            {
                                                                block = new
                                                                {
                                                                    type = "state_variable_assign",
                                                                    id = "inc_score_ties",
                                                                    fields = new Dictionary<string, object>
                                                                    {
                                                                        ["NAME"] = "_scoreTies",
                                                                        ["VALUE"] = "_scoreTies + 1"
                                                                    },
                                                                    next = new
                                                                    {
                                                                        block = new
                                                                        {
                                                                            type = "state_variable_assign",
                                                                            id = "set_status_draw",
                                                                            fields = new Dictionary<string, object>
                                                                            {
                                                                                ["NAME"] = "_txtStatusText",
                                                                                ["VALUE"] = "\"Game Drawn!\""
                                                                            },
                                                                            next = new
                                                                            {
                                                                                block = new
                                                                                {
                                                                                    type = "state_variable_assign",
                                                                                    id = "set_score_ties_text",
                                                                                    fields = new Dictionary<string, object>
                                                                                    {
                                                                                        ["NAME"] = "_txtScoreTiesText",
                                                                                        ["VALUE"] = "\"Ties: $_scoreTies\""
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                next = new
                                                {
                                                    block = new
                                                    {
                                                        type = "dialog_action_modal",
                                                        id = "dlg_draw",
                                                        fields = new Dictionary<string, object>
                                                        {
                                                            ["TITLE"] = "🤝 Draw!",
                                                            ["MESSAGE"] = "The game ended in a draw!",
                                                            ["BTN_TEXT"] = "Play Again"
                                                        },
                                                        inputs = new Dictionary<string, object>
                                                        {
                                                            ["DO"] = new
                                                            {
                                                                block = new
                                                                {
                                                                    type = "logic_call_function",
                                                                    id = "call_restart_draw",
                                                                    fields = new Dictionary<string, object>
                                                                    {
                                                                        ["FUNC_NAME"] = "_restartGame",
                                                                        ["ARGS"] = ""
                                                                    }
                                                                }
                                                            }
                                                        },
                                                        next = new
                                                        {
                                                            block = new
                                                            {
                                                                type = "logic_return",
                                                                id = "ret_draw",
                                                                fields = new Dictionary<string, object>()
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        ["DO_ELSE"] = new
                                        {
                                            block = new
                                            {
                                                type = "set_state_wrapper",
                                                id = "setstate_toggle_turn",
                                                inputs = new Dictionary<string, object>
                                                {
                                                    ["DO"] = new
                                                    {
                                                        block = new
                                                        {
                                                            type = "state_variable_assign",
                                                            id = "assign_toggle_player",
                                                            fields = new Dictionary<string, object>
                                                            {
                                                                ["NAME"] = "_currentPlayer",
                                                                ["VALUE"] = "_currentPlayer == 'X' ? 'O' : 'X'"
                                                            },
                                                            next = new
                                                            {
                                                                block = new
                                                                {
                                                                    type = "state_variable_assign",
                                                                    id = "assign_toggle_status",
                                                                    fields = new Dictionary<string, object>
                                                                    {
                                                                        ["NAME"] = "_txtStatusText",
                                                                        ["VALUE"] = "\"Player $_currentPlayer's Turn\""
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            allOverviewBlocks.Add(checkWinnerBlockObj);

            var restartGameBlockObj = new
            {
                type = "logic_function",
                id = "block_func_restartGame",
                x = 1140,
                y = 1200,
                fields = new Dictionary<string, object>
                {
                    ["FUNC_NAME"] = "_restartGame()"
                },
                inputs = new Dictionary<string, object>
                {
                    ["BODY"] = new
                    {
                        block = new
                        {
                            type = "set_state_wrapper",
                            id = "setstate_restart",
                            inputs = new Dictionary<string, object>
                            {
                                ["DO"] = new
                                {
                                    block = new
                                    {
                                        type = "state_variable_assign",
                                        id = "assign_board_reset",
                                        fields = new Dictionary<string, object>
                                        {
                                            ["NAME"] = "_board",
                                            ["VALUE"] = "List.filled(9, '')"
                                        },
                                        next = new
                                        {
                                            block = new
                                            {
                                                type = "state_variable_assign",
                                                id = "assign_player_reset",
                                                fields = new Dictionary<string, object>
                                                {
                                                    ["NAME"] = "_currentPlayer",
                                                    ["VALUE"] = "'X'"
                                                },
                                                next = new
                                                {
                                                    block = new
                                                    {
                                                        type = "state_variable_assign",
                                                        id = "assign_gameover_reset",
                                                        fields = new Dictionary<string, object>
                                                        {
                                                            ["NAME"] = "_gameOver",
                                                            ["VALUE"] = "false"
                                                        },
                                                        next = new
                                                        {
                                                            block = new
                                                            {
                                                                type = "textview_set_text",
                                                                id = "set_status_reset_turn",
                                                                fields = new Dictionary<string, object>
                                                                {
                                                                    ["WIDGET_ID"] = "txt_status",
                                                                    ["TEXT"] = "Player X's Turn"
                                                                },
                                                                next = new
                                                                {
                                                                    block = new
                                                                    {
                                                                        type = "button_clear_all_cells",
                                                                        id = "clear_cells_reset",
                                                                        fields = new Dictionary<string, object>()
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            allOverviewBlocks.Add(restartGameBlockObj);

            // 6d. Generate main overview workspace (workspace.json and main.json)
            var overviewRoot = new
            {
                blocks = new
                {
                    languageVersion = 0,
                    blocks = allOverviewBlocks
                }
            };
            string overviewJson = JsonSerializer.Serialize(overviewRoot, JsonOptions);
            await File.WriteAllTextAsync(Path.Combine(metaPath, "workspace.json"), overviewJson);
            await File.WriteAllTextAsync(Path.Combine(pagesDir, "main.json"), overviewJson);

            // 6e. Generate dedicated main_logic.event.dart and main.event.dart
            string mainLogicDart = GetTicTacToeLogicDart();
            await File.WriteAllTextAsync(Path.Combine(pagesDir, "main_logic.event.dart"), mainLogicDart);
            await File.WriteAllTextAsync(Path.Combine(pagesDir, "main.event.dart"), mainLogicDart);

            // 7. Compile the Flutter Dart code
            string combinedEventCode = await ProjectService.LoadAllPageEventCodeAsync(projectRoot, "main");
            string completeDartSource = FlutterCompilerService.GenerateCompletePageDart("main", uiJson, combinedEventCode, meta);

            await File.WriteAllTextAsync(Path.Combine(flutterLibDir, "main.dart"), completeDartSource);
            await File.WriteAllTextAsync(Path.Combine(legacyLibDir, "main.dart"), completeDartSource);
        }

        private static string GetTicTacToeUiJson()
        {
            var rootColumn = new
            {
                id = "root_column",
                name = "Root Layout",
                type = "Column",
                width = "Match Parent",
                height = "Match Parent",
                backgroundColor = "#0F172A",
                mainAxisAlignment = "Center",
                crossAxisAlignment = "Center",
                padding = "16",
                children = new object[]
                {
                    // 1. Header & Scoreboard Card
                    new
                    {
                        id = "card_header",
                        name = "Scoreboard Card",
                        type = "Card",
                        width = "300",
                        height = "Auto",
                        backgroundColor = "#1E293B",
                        cornerRadius = "16",
                        padding = "12",
                        margin = "8",
                        hasShadow = true,
                        children = new object[]
                        {
                            new
                            {
                                id = "col_header",
                                name = "Header Content",
                                type = "Column",
                                width = "Match Parent",
                                height = "Auto",
                                padding = "0",
                                margin = "0",
                                crossAxisAlignment = "Center",
                                children = new object[]
                                {
                                    new
                                    {
                                        id = "txt_status",
                                        name = "Game Status",
                                        type = "Text",
                                        text = "Player X's Turn",
                                        textColor = "#38BDF8",
                                        textSize = "20",
                                        fontWeight = "Bold",
                                        textAlign = "Center",
                                        padding = "0",
                                        margin = "0,0,0,8"
                                    },
                                    new
                                    {
                                        id = "row_scores",
                                        name = "Scores Row",
                                        type = "Row",
                                        width = "Match Parent",
                                        height = "Auto",
                                        padding = "0",
                                        margin = "0",
                                        mainAxisAlignment = "SpaceEvenly",
                                        children = new object[]
                                        {
                                            new { id = "txt_score_x", name = "Player X Score", type = "Text", text = "Player X: 0", textColor = "#F43F5E", textSize = "14", fontWeight = "Bold", padding = "0", margin = "0" },
                                            new { id = "txt_score_o", name = "Player O Score", type = "Text", text = "Player O: 0", textColor = "#10B981", textSize = "14", fontWeight = "Bold", padding = "0", margin = "0" },
                                            new { id = "txt_score_ties", name = "Ties Score", type = "Text", text = "Ties: 0", textColor = "#94A3B8", textSize = "14", fontWeight = "Bold", padding = "0", margin = "0" }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // 2. Board Container (300x300)
                    new
                    {
                        id = "board_container",
                        name = "Board Container",
                        type = "Container",
                        width = "300",
                        height = "300",
                        backgroundColor = "#1E293B",
                        cornerRadius = "16",
                        padding = "8",
                        margin = "12",
                        hasShadow = true,
                        children = new object[]
                        {
                            new
                            {
                                id = "board_grid",
                                name = "Board Grid",
                                type = "Column",
                                width = "Match Parent",
                                height = "Match Parent",
                                padding = "0",
                                margin = "0",
                                mainAxisAlignment = "SpaceEvenly",
                                crossAxisAlignment = "Center",
                                children = new object[]
                                {
                                    // Row 0
                                    new
                                    {
                                        id = "row_0",
                                        name = "Board Row 0",
                                        type = "Row",
                                        width = "Match Parent",
                                        height = "Auto",
                                        padding = "0",
                                        margin = "0",
                                        mainAxisAlignment = "SpaceEvenly",
                                        children = new object[]
                                        {
                                            CreateCellButton("btn_cell_0", "Cell (0,0)"),
                                            CreateCellButton("btn_cell_1", "Cell (0,1)"),
                                            CreateCellButton("btn_cell_2", "Cell (0,2)")
                                        }
                                    },
                                    // Row 1
                                    new
                                    {
                                        id = "row_1",
                                        name = "Board Row 1",
                                        type = "Row",
                                        width = "Match Parent",
                                        height = "Auto",
                                        padding = "0",
                                        margin = "0",
                                        mainAxisAlignment = "SpaceEvenly",
                                        children = new object[]
                                        {
                                            CreateCellButton("btn_cell_3", "Cell (1,0)"),
                                            CreateCellButton("btn_cell_4", "Cell (1,1)"),
                                            CreateCellButton("btn_cell_5", "Cell (1,2)")
                                        }
                                    },
                                    // Row 2
                                    new
                                    {
                                        id = "row_2",
                                        name = "Board Row 2",
                                        type = "Row",
                                        width = "Match Parent",
                                        height = "Auto",
                                        padding = "0",
                                        margin = "0",
                                        mainAxisAlignment = "SpaceEvenly",
                                        children = new object[]
                                        {
                                            CreateCellButton("btn_cell_6", "Cell (2,0)"),
                                            CreateCellButton("btn_cell_7", "Cell (2,1)"),
                                            CreateCellButton("btn_cell_8", "Cell (2,2)")
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // 3. Restart Game Button
                    new
                    {
                        id = "btn_reset",
                        name = "Restart Game Button",
                        type = "Button",
                        text = "Restart Game",
                        width = "220",
                        height = "48",
                        backgroundColor = "#2563EB",
                        textColor = "#FFFFFF",
                        textSize = "16",
                        fontWeight = "Bold",
                        cornerRadius = "24",
                        margin = "12",
                        padding = "0"
                    }
                }
            };

            return JsonSerializer.Serialize(new[] { rootColumn }, JsonOptions);
        }

        private static object CreateCellButton(string id, string name)
        {
            return new
            {
                id = id,
                name = name,
                type = "Button",
                text = " ",
                width = "80",
                height = "80",
                cornerRadius = "12",
                backgroundColor = "#334155",
                textColor = "#FFFFFF",
                textSize = "32",
                fontWeight = "Bold",
                margin = "4",
                padding = "0"
            };
        }

        private static (string json, object blockObj) GetCellClickBlocklyWorkspace(string cellId, int index, int x, int y)
        {
            string camelCell = FlutterCompilerService.ToCamelCase(cellId);
            var blockObj = new
            {
                type = "widget_on_click",
                id = $"event_{cellId}_click",
                x = x,
                y = y,
                deletable = false,
                fields = new Dictionary<string, object>
                {
                    ["WIDGET_ID"] = cellId
                },
                inputs = new Dictionary<string, object>
                {
                    ["DO"] = new
                    {
                        block = new
                        {
                            type = "logic_if_else",
                            id = $"if_empty_{cellId}",
                            fields = new Dictionary<string, object>
                            {
                                ["CONDITION"] = $"!_gameOver && (_{camelCell}Text == null || _{camelCell}Text == ' ' || _{camelCell}Text == '')"
                            },
                            inputs = new Dictionary<string, object>
                            {
                                ["DO_IF"] = new
                                {
                                    block = new
                                    {
                                        type = "ui_haptic_feedback",
                                        id = $"haptic_{cellId}",
                                        fields = new Dictionary<string, object>
                                        {
                                            ["TYPE"] = "lightImpact"
                                        },
                                        next = new
                                        {
                                            block = new
                                            {
                                                type = "logic_call_function",
                                                id = $"call_move_{index}",
                                                fields = new Dictionary<string, object>
                                                {
                                                    ["FUNC_NAME"] = "_makeMove",
                                                    ["ARGS"] = $"{index}, context"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var root = new
            {
                blocks = new
                {
                    languageVersion = 0,
                    blocks = new[] { blockObj }
                }
            };

            return (JsonSerializer.Serialize(root, JsonOptions), blockObj);
        }

        private static string GetCellClickEventDart(string cellId, int index)
        {
            string camelCell = FlutterCompilerService.ToCamelCase(cellId);
            return $@"// EVENT_BINDING: onClick -> {cellId}
if (!_gameOver && (_{camelCell}Text == null || _{camelCell}Text == ' ' || _{camelCell}Text == '')) {{
  HapticFeedback.lightImpact();
  _makeMove({index}, context);
}}
";
        }

        private static (string json, object blockObj) GetResetClickBlocklyWorkspace(int x, int y)
        {
            var blockObj = new
            {
                type = "widget_on_click",
                id = "event_reset_click",
                x = x,
                y = y,
                deletable = false,
                fields = new Dictionary<string, object>
                {
                    ["WIDGET_ID"] = "btn_reset"
                },
                inputs = new Dictionary<string, object>
                {
                    ["DO"] = new
                    {
                        block = new
                        {
                            type = "textview_set_text",
                            id = "set_status_reset",
                            fields = new Dictionary<string, object>
                            {
                                ["WIDGET_ID"] = "txt_status",
                                ["TEXT"] = "Player X's Turn"
                            },
                            next = new
                            {
                                block = new
                                {
                                    type = "ui_haptic_feedback",
                                    id = "haptic_reset",
                                    fields = new Dictionary<string, object>
                                    {
                                        ["TYPE"] = "selectionClick"
                                    },
                                    next = new
                                    {
                                        block = new
                                        {
                                            type = "logic_call_function",
                                            id = "call_reset",
                                            fields = new Dictionary<string, object>
                                            {
                                                ["FUNC_NAME"] = "_restartGame",
                                                ["ARGS"] = ""
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var root = new
            {
                blocks = new
                {
                    languageVersion = 0,
                    blocks = new[] { blockObj }
                }
            };

            return (JsonSerializer.Serialize(root, JsonOptions), blockObj);
        }

        private static string GetTicTacToeLogicDart()
        {
            return @"// STATE_VAR: List<String> _board = List.filled(9, '');
// STATE_VAR: String _currentPlayer = 'X';
// STATE_VAR: bool _gameOver = false;
// STATE_VAR: int _scoreX = 0;
// STATE_VAR: int _scoreO = 0;
// STATE_VAR: int _scoreTies = 0;

// RAW_METHOD: _makeMove
void _makeMove(int index, BuildContext context) {
  if (_gameOver || _board[index].isNotEmpty) return;
  setState(() {
    _board[index] = _currentPlayer;
    if (index == 0) _btnCell0Text = _currentPlayer;
    if (index == 1) _btnCell1Text = _currentPlayer;
    if (index == 2) _btnCell2Text = _currentPlayer;
    if (index == 3) _btnCell3Text = _currentPlayer;
    if (index == 4) _btnCell4Text = _currentPlayer;
    if (index == 5) _btnCell5Text = _currentPlayer;
    if (index == 6) _btnCell6Text = _currentPlayer;
    if (index == 7) _btnCell7Text = _currentPlayer;
    if (index == 8) _btnCell8Text = _currentPlayer;
  });
  _checkWinner(context);
}

// RAW_METHOD: _checkWinner
void _checkWinner(BuildContext context) {
  for (var pattern in const [[0,1,2],[3,4,5],[6,7,8],[0,3,6],[1,4,7],[2,5,8],[0,4,8],[2,4,6]]) {
    if (_board[pattern[0]].isNotEmpty && _board[pattern[0]] == _board[pattern[1]] && _board[pattern[1]] == _board[pattern[2]]) {
      setState(() {
        _gameOver = true;
        _txtStatusText = 'Player ${_board[pattern[0]]} Wins!';
        if (_board[pattern[0]] == 'X') {
          _scoreX = _scoreX + 1;
          _txtScoreXText = 'Player X: $_scoreX';
        } else {
          _scoreO = _scoreO + 1;
          _txtScoreOText = 'Player O: $_scoreO';
        }
      });
      showDialog(
        context: context,
        barrierDismissible: false,
        builder: (dialogCtx) => AlertDialog(
          title: Text('🎉 Winner!'),
          content: Text('Player ${_board[pattern[0]]} has won the game!'),
          actions: [
            ElevatedButton(
              onPressed: () {
                Navigator.of(dialogCtx).pop();
                _restartGame();
              },
              child: Text('Play Again'),
            ),
          ],
        ),
      );
      return;
    }
  }
  if (!_board.contains('')) {
    setState(() {
      _gameOver = true;
      _scoreTies = _scoreTies + 1;
      _txtStatusText = 'Game Drawn!';
      _txtScoreTiesText = 'Ties: $_scoreTies';
    });
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (dialogCtx) => AlertDialog(
        title: Text('🤝 Draw!'),
        content: Text('The game ended in a draw!'),
        actions: [
          ElevatedButton(
            onPressed: () {
              Navigator.of(dialogCtx).pop();
              _restartGame();
            },
            child: Text('Play Again'),
          ),
        ],
      ),
    );
    return;
  } else {
    setState(() {
      _currentPlayer = _currentPlayer == 'X' ? 'O' : 'X';
      _txtStatusText = ""Player $_currentPlayer's Turn"";
    });
  }
}

// RAW_METHOD: _restartGame
void _restartGame() {
  setState(() {
    _board = List.filled(9, '');
    _currentPlayer = 'X';
    _gameOver = false;
    _btnCell0Text = ' ';
    _btnCell1Text = ' ';
    _btnCell2Text = ' ';
    _btnCell3Text = ' ';
    _btnCell4Text = ' ';
    _btnCell5Text = ' ';
    _btnCell6Text = ' ';
    _btnCell7Text = ' ';
    _btnCell8Text = ' ';
    _txtStatusText = ""Player X's Turn"";
  });
}
";
        }
    }
}
