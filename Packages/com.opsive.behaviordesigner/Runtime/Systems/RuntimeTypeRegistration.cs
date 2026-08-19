#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.BakedBehaviorTree))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.BakedEditorReference))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.DeferredBakedBehaviorTreeStart))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.EditorBehaviorTreeGraphReference))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.TaskObjectComponent))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.TaskObjectFlag))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.TaskObjectReevaluateFlag))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.TaskComponent))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.EvaluationComponent32))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.EvaluationComponent64))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.EvaluationComponent128))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.EvaluationComponent512))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.EvaluationComponent4096))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.BranchComponent))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.EvaluateFlag))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.EnabledFlag))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.InterruptFlag))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.InterruptedFlag))]
[assembly: Unity.Entities.RegisterGenericComponentType(typeof(Opsive.BehaviorDesigner.Runtime.Components.ReevaluateTaskComponent))]
#endif