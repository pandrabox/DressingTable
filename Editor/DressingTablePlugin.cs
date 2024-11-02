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
            new DressingClear(TopAvatar);
            new DressingTableMain(TopAvatar);
        }
        [MenuItem("PanDbg/DressingClear")]
        public static void DbgDressingClear()
        {
            SetDebugMode(true);
            new DressingClear(TopAvatar);
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
        public DressingTableMain(VRCAvatarDescriptor desc)
        {
            DressingTable tgt = desc.transform.GetComponentInChildren<DressingTable>(false);
            new DressingTarget(tgt).Execute();
        }
    }
    public class DressingClear ///FORDEBUG
    {
        public PandraProject Prj;
        public DressingTarget DressingTarget;
        public DressingClear(VRCAvatarDescriptor desc)
        {
            Prj = new PandraProject(desc, PROJECTNAME, PROJECTTYPE);
            DressingTable tgt = Prj.RootTransform.GetComponentInChildren<DressingTable>(false);
            DressingTarget = new DressingTarget(tgt);
            DressingTarget.Mat1st.SetFloat("_UseMain2ndTex", 0);
            DressingTarget.Mat1st.SetFloat("_UseMain3rdTex", 0);
        }
    }
}