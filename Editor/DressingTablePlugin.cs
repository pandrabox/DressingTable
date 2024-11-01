using nadena.dev.ndmf;
using com.github.pandrabox.pandravase.editor;
using com.github.pandrabox.pandravase.runtime;
using com.github.pandrabox.dressingtable.editor;
using com.github.pandrabox.dressingtable.runtime;
using static com.github.pandrabox.pandravase.runtime.Util;
using static com.github.pandrabox.pandravase.runtime.Global;
using static com.github.pandrabox.dressingtable.runtime.Global;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using UnityEditor;
using System.Linq;
using UnityEngine.Animations;
using System.Collections.Generic;
using System;

[assembly: ExportsPlugin(typeof(DressingTablePlugin))]

namespace com.github.pandrabox.dressingtable.editor
{
#if PANDRADBG
    public partial class PanDebug
    {
        [MenuItem("PanDbg/DressingTablePlugin")]
        public static void DbgDressingTablePlugin() {
            SetDebugMode(true);
            new DressingTableMain(TopAvatar);
        }
    }
#endif
    public class DressingTablePlugin : Plugin<DressingTablePlugin>
    {
        public override string QualifiedName => "com.github.pandrabox.dressingtable";
        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .BeforePlugin("nadena.dev.modular-avatar")
                .BeforePlugin("com.github.pandrabox.pandravase")
                .Run(PROJECTNAME, ctx =>
                {
                    DressingTable tgt = ctx.AvatarRootTransform.GetComponentInChildren<DressingTable>(false);
                    if (tgt == null) return;
                    new DressingTableMain(ctx.AvatarDescriptor);
                });
        }
    }
    public class DressingTableMain
    {
        string _currentTargetForErrorReport;
        //PoseClipMap _map;

        public DressingTableMain(VRCAvatarDescriptor desc)
        {
            //_map = new PoseClipMap(desc);
            //if (_map == null)
            //{
            //    ExceptionCall("mapの取得に失敗しました。予期しないエラーです。");
            //    return;
            //}
            //if (_map.AnyError)
            //{
            //    ExceptionCall(_map.ErrMsg);
            //    return;
            //}

            //// ParentConstraint設定
            //ParentConstraintSetting();

            //// ScaleConstraint設定
            //SetupScaleConstraint();
        }


        //private void ParentConstraintSetting()
        //{
        //    foreach (PoseClipUnitMap map in _map.PoseClipUnitMaps)
        //    {
        //        Transform poseClipperTransform = map.PoseClipperTransform;
        //        if (poseClipperTransform == null) ExceptionCall($@"poseClipperTransform. {_currentTargetForErrorReport}");
        //        Transform targetTransform = map.TargetTransform;

        //        if (poseClipperTransform == null) ExceptionCall($@"poseClipperTransform is null. {_currentTargetForErrorReport}");
        //        if (targetTransform == null) ExceptionCall($@"targetTransform is null. {_currentTargetForErrorReport}");

        //        var targetGameObject = targetTransform.gameObject;
        //        if (targetGameObject == null) ExceptionCall($@"targetGameObject is null. {_currentTargetForErrorReport}");
        //        var parentConstraint = targetGameObject.AddComponent<ParentConstraint>();
        //        if (targetTransform == null) ExceptionCall($@"parentConstraint is null. {_currentTargetForErrorReport}");

        //        parentConstraint.enabled = false;
        //        ConstraintSource constraintSource = new ConstraintSource();
        //        constraintSource.sourceTransform = poseClipperTransform;
        //        constraintSource.weight = 1;
        //        parentConstraint.AddSource(constraintSource);

        //        parentConstraint.constraintActive = true;
        //        parentConstraint.SetTranslationOffset(0, Vector3.zero);
        //        parentConstraint.SetRotationOffset(0, Vector3.zero);
        //    }
        //}

        //private void SetupScaleConstraint()
        //{
        //    var headBone = _map.AvatarHeadTransform;
        //    if (headBone == null) ExceptionCall("headBone is null");

        //    var scaleConstraint = headBone.gameObject.AddComponent<ScaleConstraint>();
        //    if (scaleConstraint == null) ExceptionCall("scaleConstraint is null");

        //    ConstraintSource constraintSource = new ConstraintSource();
        //    constraintSource.sourceTransform = _map.PoseClipperScaleTransform;
        //    constraintSource.weight = 1;
        //    scaleConstraint.AddSource(constraintSource);

        //    scaleConstraint.constraintActive = true;

        //    var measure = new GameObject("measure").transform;
        //    measure.SetParent(headBone.transform);
        //    measure.localScale = new Vector3(1, 1, 1);
        //    measure.SetParent(_map.PoseClipperScaleTransform);
        //    scaleConstraint.locked = false;
        //    scaleConstraint.scaleOffset = measure.localScale;
        //    scaleConstraint.enabled = false;
        //    GameObject.DestroyImmediate(measure.gameObject);
        //}

        //private void ExceptionCall(string message)
        //{
        //    _map.Prj.DebugPrint(message, false, LogType.Exception);
        //}

    }
}