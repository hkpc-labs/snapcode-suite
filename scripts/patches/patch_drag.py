import re
with open('Views/DesignerNodeControl.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()

drop_replacement = '''        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            if (_layoutContainer != null && 
                e.DataView.Properties.TryGetValue("WidgetType", out var typeObj))
            {
                if (e.DataView.Properties.TryGetValue("ExistingWidgetId", out var existingIdObj))
                {
                    // Moving an existing widget
                    string existingId = existingIdObj.ToString();
                    var existingWidget = _parentDesigner.FindWidgetById(_parentDesigner.CanvasItems, existingId);
                    if (existingWidget != null && existingWidget != NodeItem)
                    {
                        // Prevent dropping a widget into itself or its own children
                        if (_parentDesigner.FindWidgetById(existingWidget.Children, NodeItem.Id) == null)
                        {
                            _parentDesigner.RemoveWidgetFromTree(_parentDesigner.CanvasItems, existingWidget);
                            NodeItem.Children.Add(existingWidget);
                            _parentDesigner.RenderRoot();
                            _parentDesigner.SaveStateForUndo();
                            _parentDesigner.OnWidgetTreeChanged(existingWidget);
                        }
                    }
                }
                else if (e.DataView.Properties.TryGetValue("WidgetName", out var nameObj) &&
                         e.DataView.Properties.TryGetValue("WidgetIcon", out var iconObj))
                {
                    // New Widget
                    string t = typeObj.ToString() ?? "Unknown";
                    var newItem = new WidgetItem
                    {
                        Type = t,
                        Name = nameObj.ToString() ?? "Unknown",
                        Icon = iconObj.ToString() ?? "\\uE710",
                        Id = $"{t.ToLower()}_{System.Guid.NewGuid().ToString().Substring(0, 4)}"
                    };

                    NodeItem.Children.Add(newItem);
                    _parentDesigner.RenderRoot();
                    _parentDesigner.SaveStateForUndo();
                    _parentDesigner.OnWidgetTreeChanged(newItem);
                }
                e.Handled = true;
            }
        }'''

content = re.sub(r'        private void UserControl_Drop\(object sender, DragEventArgs e\)\s*\{.*?e\.Handled = true;\s*\}\s*\}', drop_replacement, content, flags=re.DOTALL)
with open('Views/DesignerNodeControl.xaml.cs', 'w', encoding='utf-8') as f: f.write(content)

with open('Views/NativeUIDesigner.xaml.cs', 'r', encoding='utf-8') as f:
    content2 = f.read()

root_drop = '''        private void RootCanvas_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Properties.TryGetValue("WidgetType", out var typeObj))
            {
                if (e.DataView.Properties.TryGetValue("ExistingWidgetId", out var existingIdObj))
                {
                    string existingId = existingIdObj.ToString();
                    var existingWidget = FindWidgetById(CanvasItems, existingId);
                    if (existingWidget != null)
                    {
                        RemoveWidgetFromTree(CanvasItems, existingWidget);
                        CanvasItems.Add(existingWidget);
                        RenderRoot();
                        SaveStateForUndo();
                        OnWidgetTreeChanged(existingWidget);
                    }
                }
                else if (e.DataView.Properties.TryGetValue("WidgetName", out var nameObj) &&
                         e.DataView.Properties.TryGetValue("WidgetIcon", out var iconObj))
                {
                    string t = typeObj.ToString() ?? "Unknown";
                    var newItem = new WidgetItem
                    {
                        Type = t,
                        Name = nameObj.ToString() ?? "Unknown",
                        Icon = iconObj.ToString() ?? "\\uE710",
                        Id = $"{t.ToLower()}_{_widgetIdCounter++}"
                    };

                    CanvasItems.Add(newItem);
                    RenderRoot();
                    SaveStateForUndo();
                    OnWidgetTreeChanged(newItem);
                }
            }
        }'''

content2 = re.sub(r'        private void RootCanvas_Drop\(object sender, DragEventArgs e\)\s*\{.*?OnWidgetTreeChanged\(newItem\);\s*\}\s*\}', root_drop, content2, flags=re.DOTALL)
with open('Views/NativeUIDesigner.xaml.cs', 'w', encoding='utf-8') as f: f.write(content2)
print('Patch applied')
