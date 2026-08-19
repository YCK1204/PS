#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.BehaviorDesigner.Runtime.Groups;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Events;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Unity.Entities;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary>
    /// Converts the behavior tree to a DOTS entity.
    /// </summary>
    [BakeDerivedTypes]
    public class BehaviorTreeBaker : Baker<BehaviorTree>
    {
        private static MethodInfo s_GetTypeOfSystemMethod;

        /// <summary>
        /// Bakes the behavior tree to the DOTS entity.
        /// </summary>
        /// <param name="behaviorTree">The authoring behavior tree.</param>
        public override void Bake(BehaviorTree behaviorTree)
        {
            if (!behaviorTree.enabled) {
                return;
            }

            var entity = GetEntity(behaviorTree, TransformUsageFlags.Dynamic);
            var worlds = World.All;
            for (int i = 0; i < worlds.Count; ++i) {
                if (worlds[i].EntityManager.Exists(entity)) {
                    if (!behaviorTree.InitializeTree(worlds[i], entity, true)) {
                        continue;
                    }
                    RegisterSubtreeDependencies(behaviorTree);
#if UNITY_EDITOR
                    ushort[] logicNodeRuntimeIndices = null;
                    var logicNodes = behaviorTree.TreeLogicNodes;
                    if (logicNodes != null && logicNodes.Length > 0) {
                        logicNodeRuntimeIndices = new ushort[logicNodes.Length];
                        for (int j = 0; j < logicNodes.Length; ++j) {
                            logicNodeRuntimeIndices[j] = logicNodes[j].RuntimeIndex;
                        }
                    }
#endif

                    var connectedIndex = GetStartTaskConnectedIndex(behaviorTree);
                    if (connectedIndex == -1 || connectedIndex == ushort.MaxValue) {
                        return;
                    }

                    var taskComponents = worlds[i].EntityManager.GetBuffer<TaskComponent>(entity);
                    var tagStableTypeHash = new ulong[taskComponents.Length];
                    for (int j = 0; j < taskComponents.Length; ++j) {
                        tagStableTypeHash[j] = TypeManager.GetTypeInfo(taskComponents[j].FlagComponentType.TypeIndex).StableTypeHash;
                    }
                    ulong[] ReevaluateFlagStableTypeHash = null;
                    if (worlds[i].EntityManager.HasBuffer<ReevaluateTaskComponent>(entity)) {
                        var reevaluateTaskComponents = worlds[i].EntityManager.GetBuffer<ReevaluateTaskComponent>(entity);
                        ReevaluateFlagStableTypeHash = new ulong[reevaluateTaskComponents.Length];
                        for (int j = 0; j < reevaluateTaskComponents.Length; ++j) {
                            ReevaluateFlagStableTypeHash[j] = TypeManager.GetTypeInfo(reevaluateTaskComponents[j].ReevaluateFlagComponentType.TypeIndex).StableTypeHash;
                        }
                    }

                    AddComponentObject<BakedBehaviorTree>(entity, new BakedBehaviorTree
                    {
                        StartEventConnectedIndex = connectedIndex,
                        StartWhenEnabled = behaviorTree.StartWhenEnabled,
                        StartEvaluation = behaviorTree.UpdateMode == UpdateMode.EveryFrame,
                        ReevaluateTaskSystems = GetTaskSystems<ReevaluateTaskSystemGroup>(worlds[i]),
                        InterruptTaskSystems = GetTaskSystems<InterruptTaskSystemGroup>(worlds[i]),
                        TraversalTaskSystems = GetTaskSystems<TraversalTaskSystemGroup>(worlds[i]),
                        TagStableTypeHashes = tagStableTypeHash,
                        ReevaluateFlagStableTypeHashes = ReevaluateFlagStableTypeHash,
                    });
#if UNITY_EDITOR
                    // Stored in a separate component so StripEditorBehaviorTreeReferenceSystem can remove it during the EntitySceneOptimizations pass, keeping it out of the build.
                    AddComponentObject<BakedEditorReference>(entity, new BakedEditorReference
                    {
                        AuthoringBehaviorTreeGlobalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(behaviorTree).ToString(),
                        DesignGraphUniqueID = behaviorTree.UniqueID,
                        LogicNodeRuntimeIndices = logicNodeRuntimeIndices,
                    });
#endif
                }
            }
        }

        /// <summary>
        /// Registers all static subtrees as bake dependencies.
        /// </summary>
        /// <param name="behaviorTree">The behavior tree being baked.</param>
        private void RegisterSubtreeDependencies(BehaviorTree behaviorTree)
        {
            var visitedSubtrees = new HashSet<Subtree>();
            if (behaviorTree.m_Subtree != null && visitedSubtrees.Add(behaviorTree.m_Subtree)) {
                DependsOn(behaviorTree.m_Subtree);
            }
            RegisterSubtreeDependencies(behaviorTree.Data, visitedSubtrees);
        }

        /// <summary>
        /// Registers all injected subtrees in the specified graph data as bake dependencies.
        /// </summary>
        /// <param name="data">The graph data that may contain injected subtree references.</param>
        /// <param name="visitedSubtrees">The subtrees that have already been registered.</param>
        private void RegisterSubtreeDependencies(BehaviorTreeData data, HashSet<Subtree> visitedSubtrees)
        {
            if (data == null || data.InjectedSubtreeReferences == null) {
                return;
            }

            for (int i = 0; i < data.InjectedSubtreeReferences.Count; ++i) {
                var subtrees = data.InjectedSubtreeReferences[i].Subtrees;
                if (subtrees == null) {
                    continue;
                }

                for (int j = 0; j < subtrees.Length; ++j) {
                    if (subtrees[j] == null || !visitedSubtrees.Add(subtrees[j])) {
                        continue;
                    }
                    DependsOn(subtrees[j]);
                    RegisterSubtreeDependencies(subtrees[j].Data, visitedSubtrees);
                }
            }
        }

        /// <summary>
        /// Returns the index of the node connection for the start event task.
        /// </summary>
        /// <param name="behaviorTree">The interested behavior tree.</param>
        /// <returns>The index of the node connection for the start event task.</returns>
        private int GetStartTaskConnectedIndex(BehaviorTree behaviorTree)
        {
            // The behavior tree has to first be initialized.
            if (behaviorTree.World == null || behaviorTree.Entity == Entity.Null) {
                Debug.LogError($"Error: Unable to retrieve the connected index on behavior tree {behaviorTree}. The behavior tree has to first be initialized.");
                return -1;
            }

            var data = behaviorTree.Data;
            for (int i = 0; i < data.EventNodes.Length; ++i) {
                if (data.EventNodes[i].GetType() == typeof(Start)) {
                    // The connected index may not be valid.
                    if (data.EventNodes[i].ConnectedIndex == ushort.MaxValue) {
                        return ushort.MaxValue;
                    }

                    // The branch cannot start if it is disabled.
                    if (!behaviorTree.IsNodeEnabled(false, i)) {
                        return -1;
                    }

                    return data.LogicNodes[data.EventNodes[i].ConnectedIndex].RuntimeIndex;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns all of the system type indicies within the systems of the specified type.
        /// </summary>
        /// <param name="world">The world that the systems were added to.</param>
        /// <returns>The system type indicies within the systems of the specified type (can be null).</returns>
        private string[] GetTaskSystems<T>(World world) where T : ComponentSystemGroup
        {
            var systemGroup = world.GetExistingSystemManaged<T>();
            if (systemGroup == null) {
                return null;
            }

            var systems = systemGroup.GetAllSystems();
            if (systems.Length == 0) {
                systems.Dispose();
                return null;
            }

            // Use reflection to call WorldUnmanaged.GetTypeOfSystem. This method is only called during baking at edit time so reflection is ok, though it would be better
            // if this method was eventually made public.
            if (s_GetTypeOfSystemMethod == null) {
                s_GetTypeOfSystemMethod = typeof(WorldUnmanaged).GetMethod("GetTypeOfSystem", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (s_GetTypeOfSystemMethod == null) {
                    Debug.LogError("Error: Unable to find WorldUnmanaged.GetTypeOfSystem. Please email support@opsive.com with your Unity version and Entity package version.");
                    return null;
                }
            }

            var systemTypes = new string[systems.Length];
            for (int i = 0; i < systems.Length; ++i) {
                var systemTypeIndex = TypeManager.GetSystemTypeIndex((Type)s_GetTypeOfSystemMethod.Invoke(world.Unmanaged, new object[] { systems[i] }));
                systemTypes[i] = systemTypeIndex.ToString();
            }
            systems.Dispose();
            return systemTypes;
        }
    }
}
#endif