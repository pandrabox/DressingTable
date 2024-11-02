using nadena.dev.ndmf;
using com.github.pandrabox.pandravase.editor;
using com.github.pandrabox.pandravase.runtime;
using com.github.pandrabox.dressingtable.editor;
using com.github.pandrabox.dressingtable.runtime;
using static com.github.pandrabox.pandravase.runtime.Util;
using static com.github.pandrabox.dressingtable.runtime.Global;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using UnityEditor;
using System.Linq;
using UnityEngine.Animations;
using System.Collections.Generic;
using System;
using nadena.dev.modular_avatar.core;
using static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control;
using System.IO;
using UnityEditor.Animations;

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
        DressingTable DressingTable;
        DressingTarget DressingTarget;
        PandraProject Prj;

        public DressingTableMain(VRCAvatarDescriptor desc)
        {
            Prj = new PandraProject(desc, PROJECTNAME, PROJECTTYPE);
            DressingTable = desc.transform.GetComponentInChildren<DressingTable>(false);
            DressingTarget = new DressingTarget(DressingTable);
            DressingTarget.Execute();
            if(DressingTable.CreateMenu) CreateToggleMenu();
            CreateContactReceptor();

        }

        public void CreateContactReceptor()
        {
            if (DressingTarget.DressingMode == DressingTarget.DressingModeEnum.error) return;
            var LayerStr = DressingTarget.DressingMode == DressingTarget.DressingModeEnum.second ? "2nd" : "3rd";
            var ab = new AnimationClipsBuilder();
            ab.Clip("ContactReceptor")
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.r").Const2F(1)
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.g").Const2F(1)
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.b").Const2F(1)
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.a").Liner(0, 0, 1 / FPS, 1);
            ab.Clip("OFF")
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.r").Const2F(1)
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.g").Const2F(1)
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.b").Const2F(1)
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.a").Const2F(0);
            string contactParam;
            contactParam = DressingTable.LinkContact.parameter;
            if (string.IsNullOrEmpty(contactParam))
            {
                contactParam = Guid.NewGuid().ToString();
                DressingTable.LinkContact.parameter=contactParam;
            }
            Directory.CreateDirectory(Prj.WorkFolder);
            if (DressingTable.CreateMenu)
            {
                AssetDatabase.CopyAsset($@"{Prj.AssetsFolder}ContactReceptorWithSW.controller", $@"{Prj.WorkFolder}ContactReceptor.controller");
            }
            else
            {
                AssetDatabase.CopyAsset($@"{Prj.AssetsFolder}ContactReceptor.controller", $@"{Prj.WorkFolder}ContactReceptor.controller");
            }
            AnimatorController contactReceptor = AssetDatabase.LoadAssetAtPath<AnimatorController>($@"{Prj.WorkFolder}ContactReceptor.controller");
            AnimatorState contactState =  contactReceptor.layers[0].stateMachine.states.FirstOrDefault(s => s.state.name == "Contact").state;
            contactState.motion = ab.Outp("ContactReceptor");
            if (DressingTable.CreateMenu)
            {
                AnimatorState offState = contactReceptor.layers[0].stateMachine.states.FirstOrDefault(s => s.state.name == "OFF").state;
                offState.motion = ab.Outp("OFF");
            }
            AnimatorControllerParameter parameter = new AnimatorControllerParameter
            {
                name = contactParam,
                type = AnimatorControllerParameterType.Float
            };
            contactReceptor.AddParameter(parameter);
            contactState.timeParameter = contactParam;
            Prj.CreateComponentObject<ModularAvatarMergeAnimator>("ContactReceptor", (x) => {
                x.animator = contactReceptor;
                x.matchAvatarWriteDefaults = true;
                x.relativePathRoot.Set(Prj.RootObject);
            });
        }

        private void CreateToggleSwitch()
        {
            Prj.CreateComponentObject<ModularAvatarMenuItem>("DressingSW", (x) =>
            {
                x.gameObject.AddComponent<ModularAvatarMenuInstaller>();
                x.Control.name = "DressingSW";
                x.Control.type = ControlType.Toggle;
                x.Control.parameter = new Parameter { name = Prj.GetParameterName("SW") };
                //x.Control.icon = AssetDatabase.LoadAssetAtPath<Texture2D>($@"{Prj.ImgFolder}Hue.png");
            });
            var MAP = Prj.CreateComponentObject<ModularAvatarParameters>("Parameter", (x) =>
            {
                x.parameters = new List<ParameterConfig>
                {
                    new ParameterConfig { nameOrPrefix = Prj.GetParameterName("SW"), syncType = ParameterSyncType.Bool, saved = false, localOnly = false, defaultValue = 0 }
                };
            });
        }

        public void CreateToggleMenu()
        {
            if (DressingTarget.DressingMode == DressingTarget.DressingModeEnum.error) return;
            var LayerStr = DressingTarget.DressingMode == DressingTarget.DressingModeEnum.second ? "2nd" : "3rd";
            var ab = new AnimationClipsBuilder();
            ab.Clip("OFF")
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.r").Const2F(1)
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.g").Const2F(1)
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.b").Const2F(1)
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.a").Const2F(0);
            ab.Clip("ON")
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.r").Const2F(1)
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.g").Const2F(1)
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.b").Const2F(1)
                .Bind(DressingTarget.Path, typeof(Renderer), $@"material._Color{LayerStr}.a").Const2F(1);
            var bb = new BlendTreeBuilder(Prj, false, "DressingTableToggle", Prj.RootObject);
            bb.RootDBT(() =>{
                bb.Param("1").Add1D("SW", () =>
                {
                    bb.Param(0).AddMotion(ab.Outp("OFF"));
                    bb.Param(1).AddMotion(ab.Outp("ON"));
                });
            });
            CreateToggleSwitch();
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