/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Editor.Controls
{
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals;
    using Opsive.BehaviorDesigner.Runtime.Utility;
    using Opsive.GraphDesigner.Editor.Controls;
    using Opsive.GraphDesigner.Editor.Elements;
    using Opsive.GraphDesigner.Runtime.Utility;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.Shared.Editor.UIElements.Controls.Types;
    using Opsive.Shared.Utility;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.UIElements;
    using UnityEditor;

    /// <summary>
    /// Implements TypeControlBase for the StackedAction/StackedConditional type.
    /// </summary>
    [ControlType(typeof(StackedAction))]
    [ControlType(typeof(StackedConditional))]
    public class StackedTaskControl : TypeControlBase
    {
        /// <summary>
        /// The StackedTaskView displays the contents of the StackedTask.
        /// </summary>
        private class StackedTaskView : VisualElement
        {
            private UnityEngine.Object m_UnityObject;
            private StackedTask m_StackedTask;
            private Func<object, bool> m_OnChangeEvent;
            private ReorderableList m_ReorderableList;
            private LogicNode m_LogicNode;

            private int m_SelectedIndex;
            private VisualElement m_SelectedTaskContent;

            private static object s_TaskClipboard;

            /// <summary>
            /// StackedTaskView constructor.
            /// </summary>
            /// <param name="unityObject">A reference to the owning Unity Object.</param>
            /// <param name="stackedTask">The StackedTask task being drawn.</param>
            /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
            public StackedTaskView(UnityEngine.Object unityObject, StackedTask stackedTask, Func<object, bool> onChangeEvent)
            {
                m_UnityObject = unityObject;
                m_StackedTask = stackedTask;
                m_OnChangeEvent = onChangeEvent;
                var graphWindow = BehaviorDesignerWindow.Instance;
                m_LogicNode = graphWindow.GraphView.NodeByNodeIndex[m_StackedTask.Index];

                FieldInspectorView.AddField(m_UnityObject, m_StackedTask, "m_ComparisonType", this, (object obj) =>
                {
                    m_OnChangeEvent(m_StackedTask);
                });

                m_ReorderableList = new ReorderableList(m_StackedTask.Tasks, (VisualElement parent, int index) =>
                {
                    parent.AddToClassList("horizontal-layout");

                    var label = new Label();
                    label.style.flexGrow = 1;
                    label.RegisterCallback<ContextClickEvent>(evt => {
                        var rowIndex = parent.userData != null ? (int)parent.userData : index;
                        var menu = new GenericMenu();

                        if (Application.isPlaying) {
                            menu.AddDisabledItem(new GUIContent("Replace"));
                        } else {
                            menu.AddItem(new GUIContent("Replace"), false, () => {
                                if (rowIndex >= 0 && rowIndex < m_StackedTask.Tasks.Length) {
                                    ShowReplaceTaskFilterWindow(rowIndex, label.worldBound.position);
                                }
                            });
                        }
                        menu.AddSeparator(string.Empty);

                        var taskItem = rowIndex >= 0 && rowIndex < m_StackedTask.Tasks.Length ? m_StackedTask.Tasks[rowIndex] : null;
                        var monoScript = ElementInspector.GetMonoScript(taskItem);
                        if (monoScript != null) {
                            menu.AddItem(new GUIContent("Edit Script"), false, () => {
                                OpenSelectScript(taskItem, true);
                            });
                            menu.AddItem(new GUIContent("Locate Script"), false, () => {
                                OpenSelectScript(taskItem, false);
                            });
                        } else {
                            menu.AddDisabledItem(new GUIContent("Edit Script"));
                            menu.AddDisabledItem(new GUIContent("Locate Script"));
                        }
                        menu.AddSeparator(string.Empty);
                        menu.AddItem(new GUIContent("Copy"), false, () => {
                            if (rowIndex >= 0 && rowIndex < m_StackedTask.Tasks.Length) {
                                s_TaskClipboard = CopyUtility.DeepCopy(m_StackedTask.Tasks[rowIndex]);
                            }
                        });
                        if (!Application.isPlaying && s_TaskClipboard is Task) {
                            menu.AddItem(new GUIContent("Paste"), false, () => {
                                OnActionAdd(CopyUtility.DeepCopy(s_TaskClipboard), null, Vector2.zero);
                            });
                        } else {
                            menu.AddDisabledItem(new GUIContent("Paste"));
                        }
                        if (Application.isPlaying) {
                            menu.AddDisabledItem(new GUIContent("Duplicate"));
                            menu.AddDisabledItem(new GUIContent("Delete"));
                        } else {
                            menu.AddItem(new GUIContent("Duplicate"), false, () => {
                                if (rowIndex >= 0 && rowIndex < m_StackedTask.Tasks.Length) {
                                    OnActionAdd(CopyUtility.DeepCopy(m_StackedTask.Tasks[rowIndex]), null, Vector2.zero, rowIndex);
                                }
                            });
                            menu.AddItem(new GUIContent("Delete"), false, () => {
                                if (rowIndex >= 0 && rowIndex < m_StackedTask.Tasks.Length) {
                                    RemoveSelectedTasks(rowIndex);
                                }
                            });
                        }
                        menu.ShowAsContext();
                        evt.StopPropagation();
                    });
                    parent.Add(label);

                    var toggle = new Toggle();
                    toggle.style.marginRight = 18;
                    toggle.RegisterValueChangedCallback(evt => {
                        var rowIndex = parent.userData != null ? (int)parent.userData : index;
                        var taskToUpdate = m_StackedTask.Tasks[rowIndex];
                        if (taskToUpdate != null) {
                            taskToUpdate.Enabled = evt.newValue;
                            m_OnChangeEvent(m_StackedTask);
                        }
                    });
                    parent.Add(toggle);
                }, (VisualElement parent, int index) => {
                    parent.userData = index;

                    var label = parent.ElementAt(0) as Label;
                    var toggle = parent.ElementAt(1) as Toggle;

                    var task = m_StackedTask.Tasks[index];
                    if (task == null) {
                        label.text = "(Unknown Task)";
                        toggle.SetValueWithoutNotify(false);
                    } else {
                        label.text = task.ToString();
                        toggle.SetValueWithoutNotify(task.Enabled);
                    }
                    toggle.tooltip = "Enable/Disable this action";
                }, (VisualElement parent) => {
                    var headerContainer = new VisualElement();
                    headerContainer.style.flexDirection = FlexDirection.Row;
                    headerContainer.style.flexGrow = 1;

                    var actionsLabel = new Label("Actions");
                    actionsLabel.style.flexGrow = 1;
                    headerContainer.Add(actionsLabel);

                    var enabledLabel = new Label("Enabled");
                    enabledLabel.style.marginLeft = 5;
                    enabledLabel.style.flexShrink = 0;
                    headerContainer.Add(enabledLabel);

                    parent.Add(headerContainer);
                }, (int index) => {
                    SelectTask(index);
                    graphWindow.EditorData.SetInspectorData(m_LogicNode.Guid.ToString(), index); // Remember the selected element.
                }, () => // Add.
                {
                    var isAction = stackedTask is IAction;
                    FilterWindow.ShowFilterWindow(graphWindow, new SearchWindowContext(graphWindow.position.position + m_ReorderableList.AddButton.worldBound.position), 
                                                new Type[] { isAction ? typeof(IAction) : typeof(IConditional) }, FilterWindow.FilterType.Class, isAction ? "Action" : "Condition", 
                                                false, null, (obj, type, pos) => OnActionAdd(obj, type, pos), (Type type) => { return typeof(Task).IsAssignableFrom(type); });
                }, (int index) => // Remove.
                {
                    RemoveTask(index);
                }, (int fromIndex, int toIndex) => // Reorder.
                {
                    m_LogicNode.UpdateNodeView();
                    UpdateContainedNodeTypes();
                    m_OnChangeEvent(m_StackedTask);
                    if (m_SelectedIndex == fromIndex) {
                        SelectTask(toIndex);
                    }
                    WatchedFieldUtility.MoveWatchedFieldIndex(m_StackedTask, fromIndex, toIndex);
                });

                m_ReorderableList.EnableMultiSelect = true;
                m_ReorderableList.OnReorderSelection = (IList<ReorderableListSelectionBlock> blocks, int targetIndex) =>
                {
                    var indexMap = BuildIndexMap(m_StackedTask.Tasks.Length, blocks, targetIndex);
                    OnSelectionReordered(indexMap);
                };
                m_ReorderableList.OnReorderSelectionWithMap = (IList<ReorderableListSelectionBlock> blocks, int targetIndex, int[] indexMap) =>
                {
                    OnSelectionReordered(indexMap);
                };
                m_ReorderableList.EnableAdd = m_ReorderableList.EnableRemove = m_ReorderableList.EnableReorder = !Application.isPlaying;
                m_ReorderableList.RegisterCallback<ContextClickEvent>(evt => {
                    var menu = new GenericMenu();
                    if (!Application.isPlaying && s_TaskClipboard is Task) {
                        menu.AddItem(new GUIContent("Paste"), false, () => {
                            OnActionAdd(CopyUtility.DeepCopy(s_TaskClipboard), null, Vector2.zero);
                        });
                    } else {
                        menu.AddDisabledItem(new GUIContent("Paste"));
                    }
                    menu.ShowAsContext();
                    evt.StopPropagation();
                });
                Add(m_ReorderableList);

                // The task values should be displayed beneath the ReorderableList.
                m_SelectedTaskContent = new VisualElement();
                m_SelectedTaskContent.style.display = DisplayStyle.None;
                Add(m_SelectedTaskContent);

                // When the assembly is reloaded the selected element is cleared. Keep a history of the selected element
                // so it can be selected again upon reload.
                int data;
                if ((data = graphWindow.EditorData.GetInspectorData(m_LogicNode.Guid.ToString())) != -1) {
                    m_ReorderableList.SelectedIndex = data;
                } else if (m_StackedTask.Tasks != null && m_StackedTask.Tasks.Length > 0) {
                    // An element should always be selected.
                    m_ReorderableList.SelectedIndex = 0;
                }

                UpdateContainedNodeTypes();
            }

            /// <summary>
            /// The selected tasks were reordered.
            /// </summary>
            /// <param name="indexMap">The old-to-new index map.</param>
            private void OnSelectionReordered(int[] indexMap)
            {
                if (indexMap == null) {
                    return;
                }

                m_LogicNode.UpdateNodeView();
                UpdateContainedNodeTypes();
                m_OnChangeEvent(m_StackedTask);
                WatchedFieldUtility.RemapWatchedFieldIndices(m_StackedTask, indexMap);
                GraphDesigner.Editor.Events.GraphEventHandler.Invoke(GraphDesigner.Editor.Events.GraphEventType.ContainerNodeChanged);
            }

            /// <summary>
            /// Adds a new task to the stacked task.
            /// </summary>
            /// <param name="selectedObject">The selected task type.</param>
            /// <param name="baseType">The base type that was used to find the element.</param>
            /// <param name="windowPosition">The position of the FilterWindow.</param>
            private void OnActionAdd(object selectedObject, Type baseType, Vector2 windowPosition, int insertAfterIndex = -1)
            {
                if (selectedObject == null) {
                    return;
                }

                m_StackedTask.Add(selectedObject);
                if (insertAfterIndex >= 0 && insertAfterIndex < m_StackedTask.Tasks.Length - 1) {
                    var insertIndex = insertAfterIndex + 1;
                    var lastIndex = m_StackedTask.Tasks.Length - 1;
                    var newItem = m_StackedTask.Tasks[lastIndex];
                    for (int i = lastIndex; i > insertIndex; i--) {
                        m_StackedTask.Tasks[i] = m_StackedTask.Tasks[i - 1];
                    }
                    m_StackedTask.Tasks[insertIndex] = newItem;
                }
                m_ReorderableList.Refresh(m_StackedTask.Tasks);
                m_ReorderableList.SelectedIndex = insertAfterIndex >= 0 ? insertAfterIndex + 1 : m_StackedTask.Tasks.Length - 1;
                m_LogicNode.UpdateNodeView();
                UpdateContainedNodeTypes();
                m_OnChangeEvent(m_StackedTask);

                GraphDesigner.Editor.Events.GraphEventHandler.Invoke(GraphDesigner.Editor.Events.GraphEventType.ContainerNodeChanged);
            }

            /// <summary>
            /// Shows the FilterWindow to replace the selected task.
            /// </summary>
            /// <param name="index">The index of the task to replace.</param>
            /// <param name="position">The window position for the FilterWindow.</param>
            private void ShowReplaceTaskFilterWindow(int index, Vector2 position)
            {
                var graphWindow = BehaviorDesignerWindow.Instance;
                var isAction = m_StackedTask is IAction;
                FilterWindow.ShowFilterWindow(graphWindow, new SearchWindowContext(graphWindow.position.position + position),
                                            new Type[] { isAction ? typeof(IAction) : typeof(IConditional) }, FilterWindow.FilterType.Class,
                                            isAction ? "Action" : "Condition", false, null, (object selectedObject, Type baseType, Vector2 windowPosition) =>
                                            {
                                                ReplaceTask(index, selectedObject);
                                            }, (Type type) => { return typeof(Task).IsAssignableFrom(type); });
            }

            /// <summary>
            /// Replaces the task at the specified index.
            /// </summary>
            /// <param name="index">The index of the task to replace.</param>
            /// <param name="selectedObject">The selected task type.</param>
            private void ReplaceTask(int index, object selectedObject)
            {
                if (selectedObject == null || index < 0 || index >= m_StackedTask.Tasks.Length) {
                    return;
                }

                var task = CreateTaskFromObject(selectedObject);
                if (task == null) {
                    return;
                }

                var existingTask = m_StackedTask.Tasks[index];
                if (existingTask != null) {
                    existingTask.OnDestroy();
                }
                m_StackedTask.Tasks[index] = task;

                m_ReorderableList.Refresh(m_StackedTask.Tasks);
                m_ReorderableList.SelectedIndex = index;
                m_LogicNode.UpdateNodeView();
                SelectTask(index);
                UpdateContainedNodeTypes();
                m_OnChangeEvent(m_StackedTask);

                GraphDesigner.Editor.Events.GraphEventHandler.Invoke(GraphDesigner.Editor.Events.GraphEventType.ContainerNodeChanged);
            }

            /// <summary>
            /// Creates a task from the selected object.
            /// </summary>
            /// <param name="selectedObject">The selected object to create the task from.</param>
            /// <returns>The created task.</returns>
            private Task CreateTaskFromObject(object selectedObject)
            {
                if (selectedObject == null) {
                    return null;
                }
                var originalTasks = m_StackedTask.Tasks;
                var originalLength = originalTasks != null ? originalTasks.Length : 0;

                m_StackedTask.Add(selectedObject);
                // Use Add to construct the task, then revert the array and return the newly created entry.
                var updatedTasks = m_StackedTask.Tasks;
                if (updatedTasks == null || updatedTasks.Length == 0 || updatedTasks.Length == originalLength) {
                    m_StackedTask.Tasks = originalTasks;
                    return null;
                }

                var task = updatedTasks[updatedTasks.Length - 1];
                m_StackedTask.Tasks = originalTasks;
                return task;
            }

            /// <summary>
            /// Opens or selects the specified script.
            /// </summary>
            /// <param name="task">The task to open or select.</param>
            /// <param name="open">Should the script be opened?</param>
            private void OpenSelectScript(object task, bool open)
            {
                var monoScript = ElementInspector.GetMonoScript(task);
                if (monoScript != null) {
                    if (open) {
                        AssetDatabase.OpenAsset(monoScript);
                    } else {
                        Selection.activeObject = monoScript;
                    }
                }
            }

            /// <summary>
            /// Removes the task at the specified index.
            /// </summary>
            /// <param name="index">The index of the task.</param>
            private void RemoveTask(int index)
            {
                if (index < 0 || index >= m_StackedTask.Tasks.Length) {
                    return;
                }

                m_StackedTask.Remove(index);
                m_SelectedTaskContent.Clear();
                WatchedFieldUtility.RemoveWatchedFieldIndex(m_StackedTask, index);

                m_ReorderableList.Refresh(m_StackedTask.Tasks);
                m_LogicNode.UpdateNodeView();
                UpdateContainedNodeTypes();
                m_OnChangeEvent(m_StackedTask);
                GraphDesigner.Editor.Events.GraphEventHandler.Invoke(GraphDesigner.Editor.Events.GraphEventType.ContainerNodeChanged);
            }

            /// <summary>
            /// Removes the selected tasks if the row is selected, otherwise removes the specified row.
            /// </summary>
            /// <param name="rowIndex">The row index that was targeted.</param>
            private void RemoveSelectedTasks(int rowIndex)
            {
                var selectedIndices = m_ReorderableList.SelectedIndices;
                if (selectedIndices == null || selectedIndices.Length == 0 || Array.IndexOf(selectedIndices, rowIndex) == -1) {
                    RemoveTask(rowIndex);
                    if (m_StackedTask.Tasks.Length > 0) {
                        m_ReorderableList.SelectedIndex = rowIndex == 0 ? 0 : rowIndex - 1;
                    }
                    return;
                }

                Array.Sort(selectedIndices);
                var nextSelectedIndex = selectedIndices[0];
                for (int i = selectedIndices.Length - 1; i >= 0; --i) {
                    RemoveTask(selectedIndices[i]);
                }
                if (m_StackedTask.Tasks.Length > 0) {
                    m_ReorderableList.SelectedIndex = Mathf.Clamp(nextSelectedIndex, 0, m_StackedTask.Tasks.Length - 1);
                }
            }

            /// <summary>
            /// Builds a mapping from old index to new index after a block move.
            /// </summary>
            /// <param name="count">The number of indices.</param>
            /// <param name="blocks">The blocks that moved.</param>
            /// <param name="targetIndex">The target index.</param>
            /// <returns>The old-to-new index map.</returns>
            private int[] BuildIndexMap(int count, IList<ReorderableListSelectionBlock> blocks, int targetIndex)
            {
                var indexOrder = new List<int>();
                for (int i = 0; i < count; ++i) {
                    indexOrder.Add(i);
                }
                ReorderableList.MoveSelectionBlocksInList(indexOrder, blocks, targetIndex);

                var indexMap = new int[count];
                for (int i = 0; i < indexOrder.Count; ++i) {
                    indexMap[indexOrder[i]] = i;
                }
                return indexMap;
            }

            /// <summary>
            /// Updates the contained node types for the stacked task.
            /// </summary>
            private void UpdateContainedNodeTypes()
            {
                var graphWindow = BehaviorDesignerWindow.Instance;
                if (graphWindow == null || graphWindow.Graph == null) {
                    return;
                }

                var nodeProperties = graphWindow.Graph.LogicNodeProperties;
                if (nodeProperties == null || m_StackedTask.Index >= nodeProperties.Length || nodeProperties[m_StackedTask.Index] == null) {
                    return;
                }

                var data = nodeProperties[m_StackedTask.Index].Data;
                data.ContainedNodeTypes = GraphDesigner.Editor.Views.ElementFactory.BuildContainedNodeTypes(m_StackedTask.Tasks);
                nodeProperties[m_StackedTask.Index].Data = data;
            }

            /// <summary>
            /// Selects the task that should be shown.
            /// </summary>
            /// <param name="index">The index of the task within the stacked task array.</param>
            private void SelectTask(int index)
            {
                m_SelectedIndex = index;

                m_SelectedTaskContent.Clear();
                m_SelectedTaskContent.style.display = DisplayStyle.Flex;

                var label = new Label();
                label.enableRichText = true;
                label.style.marginLeft = 3;
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
                label.text = $"<font-weight=700><size=+2>{m_StackedTask.Tasks[index]}</size></font-weight>";
                if (m_StackedTask.Tasks[index] is TaskDelegateBase taskDelegate) {
                    label.tooltip = $"{taskDelegate.ReflectedType}";
                } else {
                    label.tooltip = $"{m_StackedTask.Tasks[index].GetType()}";
                    if (m_StackedTask.Tasks[index].GetType().GetCustomAttribute<HideNameInTaskControlAttribute>() != null) {
                        label.style.display = DisplayStyle.None;
                    }
                }

                m_SelectedTaskContent.Add(label);
                if (m_StackedTask.Tasks[index] is TaskDelegateBase) {
                    // Ensure the TaskDelegateBaseControl is used for the TaskDelegateBase classes. FieldInspectorView won't pick up the TaskDelegateBaseControl
                    // by default because it is a base type and the FieldInspectorView does not search base types initially.
                    var control = ControlUtility.FindTypeControl(typeof(TaskDelegateBase), false);
                    if (control != null) {
                        var visualElement = control.GetTypeControl(new TypeControlInput() {
                            UnityObject = m_UnityObject,
                            Target = m_StackedTask.Tasks[index],
                            ArrayIndex = -1,
                            Type = typeof(TaskDelegateBase),
                            Value = m_StackedTask.Tasks[index],
                            Container = m_SelectedTaskContent,
                            OnChangeEvent = (object obj) =>
                            {
                                if (m_OnChangeEvent != null) {
                                    return m_OnChangeEvent(obj);
                                }
                                return true;
                            },
                            UserData = new object[] { m_StackedTask, index }
                        });
                        if (visualElement != null) {
                            m_SelectedTaskContent.Add(visualElement);
                            return;
                        }
                    }
                }

                FieldInspectorView.AddFields(m_UnityObject, m_StackedTask.Tasks[index], MemberVisibility.Public, m_SelectedTaskContent, (VisualElement element, System.Reflection.FieldInfo field) =>
                {
                    if (element == null) {
                        return;
                    }

                    // The field can be watched if it has a field that can be modified.
                    var bindableElementQuery = element.Query<BindableElement>();
                    var canWatchVariable = false;
                    if (field != null) {
                        bindableElementQuery.ForEach<BindableElement>((BindableElement bindableElement) =>
                        {
                            // All non-Label BindableElements can be watched.
                            if (!(bindableElement is Label)) {
                                canWatchVariable = true;
                            }
                            return bindableElement;
                        });
                    }
                    if (canWatchVariable) {
                        m_SelectedTaskContent.Add(new WatchedContent(m_StackedTask, index, field, element));
                    } else {
                        m_SelectedTaskContent.Add(element);
                    }
                }, (object value) =>
                {
                    if (m_OnChangeEvent != null) {
                        m_OnChangeEvent(m_StackedTask);
                    }
                }, null, null, true, null, LabelControl.WidthAdjustment.UnityDefault, null, new object[] { m_StackedTask, false });
            }
        }

        /// <summary>
        /// Does the control use a label?
        /// </summary>
        public override bool UseLabel { get { return false; } }

        /// <summary>
        /// Returns the control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="input">The input to the control.</param>
        /// <returns>The created control.</returns>
        protected override VisualElement GetControl(TypeControlInput input)
        {
            return new StackedTaskView(input.UnityObject, input.Value as StackedTask, input.OnChangeEvent);
        }
    }
}