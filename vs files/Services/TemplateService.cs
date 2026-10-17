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
  cupertino_icons: ^1.0.6

dev_dependencies:
  flutter_test:
    sdk: flutter
  flutter_lints: ^3.0.0

flutter:
  uses-material-design: true
";
            await File.WriteAllTextAsync(Path.Combine(flutterDir, "pubspec.yaml"), pubspecYaml);
            await File.WriteAllTextAsync(Path.Combine(projectRoot, "pubspec.yaml"), pubspecYaml);

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

            // 6c. Generate main overview workspace (workspace.json and main.json)
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

            // 6d. Generate main.event.dart containing state variables and game logic methods
            string mainEventDart = GetTicTacToeLogicDart();
            await File.WriteAllTextAsync(Path.Combine(pagesDir, "main.event.dart"), mainEventDart);

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
                        width = "340",
                        height = "Auto",
                        backgroundColor = "#1E293B",
                        cornerRadius = "16",
                        padding = "16",
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
                                        margin = "0,0,0,12"
                                    },
                                    new
                                    {
                                        id = "row_scores",
                                        name = "Scores Row",
                                        type = "Row",
                                        width = "Match Parent",
                                        mainAxisAlignment = "SpaceEvenly",
                                        children = new object[]
                                        {
                                            new { id = "txt_score_x", name = "Player X Score", type = "Text", text = "Player X: 0", textColor = "#F43F5E", textSize = "14", fontWeight = "Bold" },
                                            new { id = "txt_score_o", name = "Player O Score", type = "Text", text = "Player O: 0", textColor = "#10B981", textSize = "14", fontWeight = "Bold" },
                                            new { id = "txt_score_ties", name = "Ties Score", type = "Text", text = "Ties: 0", textColor = "#94A3B8", textSize = "14", fontWeight = "Bold" }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // 2. Board Container (320x320)
                    new
                    {
                        id = "board_container",
                        name = "Board Container",
                        type = "Container",
                        width = "320",
                        height = "320",
                        backgroundColor = "#1E293B",
                        cornerRadius = "16",
                        padding = "12",
                        margin = "16",
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
                        margin = "12"
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
                width = "84",
                height = "84",
                cornerRadius = "12",
                backgroundColor = "#334155",
                textColor = "#FFFFFF",
                textSize = "32",
                fontWeight = "Bold",
                margin = "4"
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
                                        type = "button_set_text",
                                        id = $"set_{cellId}_text",
                                        fields = new Dictionary<string, object>
                                        {
                                            ["WIDGET_ID"] = cellId,
                                            ["TEXT"] = "X"
                                        },
                                        next = new
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
                                                        type = "dart_raw_statement",
                                                        id = $"call_move_{index}",
                                                        fields = new Dictionary<string, object>
                                                        {
                                                            ["CODE"] = $"_makeMove({index}, context);"
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
  setState(() {{
    _{camelCell}Text = _currentPlayer;
  }});
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
                                            type = "dart_raw_statement",
                                            id = "call_reset",
                                            fields = new Dictionary<string, object>
                                            {
                                                ["CODE"] = "_restartGame();"
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
  const winPatterns = [
    [0, 1, 2], [3, 4, 5], [6, 7, 8],
    [0, 3, 6], [1, 4, 7], [2, 5, 8],
    [0, 4, 8], [2, 4, 6]
  ];
  for (var pattern in winPatterns) {
    String a = _board[pattern[0]];
    String b = _board[pattern[1]];
    String c = _board[pattern[2]];
    if (a.isNotEmpty && a == b && b == c) {
      setState(() {
        _gameOver = true;
        _txtStatusText = 'Player $a Wins!';
        if (a == 'X') {
          _scoreX++;
          _txtScoreXText = 'Player X: $_scoreX';
        } else {
          _scoreO++;
          _txtScoreOText = 'Player O: $_scoreO';
        }
      });
      showDialog(
        context: context,
        builder: (ctx) => AlertDialog(
          title: const Text('🎉 Winner!'),
          content: Text('Player $a has won the game!'),
          actions: [
            TextButton(
              onPressed: () {
                Navigator.of(ctx).pop();
                _restartGame();
              },
              child: const Text('Play Again'),
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
      _scoreTies++;
      _txtStatusText = 'Game Drawn!';
      _txtScoreTiesText = 'Ties: $_scoreTies';
    });
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('🤝 Draw!'),
        content: const Text('The game ended in a draw!'),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.of(ctx).pop();
              _restartGame();
            },
            child: const Text('Play Again'),
          ),
        ],
      ),
    );
    return;
  }
  setState(() {
    _currentPlayer = _currentPlayer == 'X' ? 'O' : 'X';
    _txtStatusText = ""Player $_currentPlayer's Turn"";
  });
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
