﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using com.github.pandrabox.pandravase.runtime;
using static com.github.pandrabox.pandravase.runtime.Util;
using static com.github.pandrabox.pandravase.runtime.Global;
using static com.github.pandrabox.dressingtable.runtime.Global;
using System.Linq;
using VRC.SDK3.Avatars.Components;
using System.Security.Cryptography;
using System.IO;
using System;
using System.Reflection;
using System.Collections;

namespace com.github.pandrabox.dressingtable.runtime
{
    public class DressingTarget
    {
        public readonly string Path;
        public readonly GameObject GameObject;
        public readonly Renderer Renderer;
        public readonly Material Mat1st;
        public string PropNameDressEnable { get; private set; }
        public string PropNameDressTex { get; private set; }
        public string PropNameDressBlendMode { get; private set; }
        public DressingModeEnum DressingMode { get; private set; }
        public DressingTable Target;
        const string sampleObjName = "DressingSample";
        public string ErrorMsg { get; private set; }
        public string WarningMsg { get; private set; }

        public enum DressingModeEnum
        {
            error,
            second,
            third
        }

        public DressingTarget(DressingTable tgt, GameObject sampleObj = null)
        {
            Target = tgt;
            if (tgt == null) PutError("Dressing情報が見つかりません。");
            ErrorMsg = "";
            WarningMsg = "";
            Path = tgt.Path;
            VRCAvatarDescriptor desc = GetAvatarDescriptor(tgt.gameObject);
            if (desc == null) PutError("アバターが見つかりません。");
            if (sampleObj == null)//通常動作
            {
                GameObject = desc?.transform.Find(Path)?.gameObject;
            }
            else//サンプル表示用
            {
                GameObject = sampleObj;
            }
            if (GameObject == null) PutError($@"GameObjectが見つかりません。指定パス{Path}を確認してください。");
            Renderer = GameObject?.GetComponent<Renderer>();
            if (Renderer == null) PutError($@"Rendererが見つかりません。指定パス{Path}のオブジェクトはRendererを有しているものを指定してください。");
            if (Renderer != null)
            {
                if (sampleObj == null)//本番環境
                {
                    Mat1st = Renderer?.materials?.Length > 0 ? Renderer?.materials[0] : null;
                    if (Renderer?.materials.Length == 0) PutError($@"Rendererにマテリアルが指定されていません。マテリアルを指定してください。　{Path}");
                }
                else // サンプル環境
                {
                    Mat1st = Renderer?.sharedMaterials.Length > 0 ? Renderer.sharedMaterials[0] : null;
                    if (Renderer?.sharedMaterials.Length == 0) PutError($@"Rendererにマテリアルが指定されていません。マテリアルを指定してください。　{Path}");
                }
            }
            if (Mat1st == null) PutError($@"マテリアルの取得に失敗しました。　{Path}");
            if (Mat1st?.shader == null) PutError($@"シェーダーに異常があります。　{Path}");
            if (Mat1st?.shader?.name?.Contains("lilToon") != true) PutError($@"lilToonシェーダーを使ってください　{Path}");
            if (ErrorMsg == "")
            {
                if (CheckProperty("_UseMain2ndTex", "_Main2ndTex", "_Main2ndTexBlendMode"))
                {
                    DressingMode = DressingModeEnum.second;
                }
                else if (CheckProperty("_UseMain3rdTex", "_Main3rdTex", "_Main3rdTexBlendMode"))
                {
                    DressingMode = DressingModeEnum.third;
                }
            }
            else
            {
                return;
            }
            if (DressingMode == DressingModeEnum.error) PutError($@"適切な空きスロットがありませんでした。2ndテクスチャ、3rdテクスチャのいずれかが空いていることを確認してください。　{Path}");
            if (tgt.LinkContact != null)
            {
                var contact = tgt.LinkContact;
                if (contact.allowSelf && contact.collisionTags.Contains("Head")) PutWarning($@"Contactの設定がAllowSelf && Headになっています。常時表示になるため、どちらかOFFを検討して下さい。");
                if (!contact.allowOthers) PutWarning($@"ContactのAllowOthersがOffになっています。通常、ONを推奨します。");
                if (!contact.collisionTags.Contains("HandL")) PutWarning($@"ContactのCollisionTagにHandLが設定されていません。設定を推奨します。");
                if (!contact.collisionTags.Contains("HandR")) PutWarning($@"ContactのCollisionTagにHandRが設定されていません。設定を推奨します。");
                if (contact.localOnly) PutWarning($@"ContactがlocalOnlyに設定されているため、自分以外に見えません。localOnly=falseを推奨します。");
                if (contact.receiverType!=VRC.Dynamics.ContactReceiver.ReceiverType.Proximity) PutWarning($@"ContactのReceiverTypeがProximity以外に設定されています。通常、Proximityを推奨します。");
            }
            if (CheckLightLimitChangerColorTempControl(desc)) PutWarning("非互換アセットLightLimitChangerの色温度調整機能ONを検出しました。頬紅が常時100%ONで固定される現象が発生します。問題なければ、LightLimitChangerの色温度調整機能をOFFにして下さい。");
        }


        private bool CheckLightLimitChangerColorTempControl(VRCAvatarDescriptor desc)
        {
            Type llcType = Type.GetType("io.github.azukimochi.LightLimitChangerSettings, io.github.azukimochi.light-limit-changer");
            if (llcType != null)
            {
                MethodInfo getComponentsMethod = typeof(Transform).GetMethod("GetComponentsInChildren", new Type[] { typeof(bool) });
                MethodInfo genericMethod = getComponentsMethod.MakeGenericMethod(llcType);
                var components = (IEnumerable)genericMethod.Invoke(desc.transform, new object[] { false });
                foreach (var component in components)
                {
                    FieldInfo parametersField = llcType.GetField("Parameters");
                    if (parametersField != null)
                    {
                        var parameters = parametersField.GetValue(component);
                        if (parameters != null)
                        {
                            FieldInfo allowColorTempControlField = parameters.GetType().GetField("AllowColorTempControl");
                            if (allowColorTempControlField != null)
                            {
                                bool allowColorTempControl = (bool)allowColorTempControlField.GetValue(parameters);
                                if (allowColorTempControl)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private GameObject SampleObject => Target?.transform?.Find(sampleObjName)?.gameObject;

        public void SetEnable()
        {
            LowLevelDebugPrint($@"{DressingMode}");
            if (DressingMode == DressingModeEnum.error) return;
            Mat1st.SetFloat(PropNameDressEnable, 1);
        }
        public void SetTexture(Texture2D tex)
        {
            if (DressingMode == DressingModeEnum.error) return;
            Mat1st.SetTexture(PropNameDressTex, tex);
        }
        public void SetBlendMode(float blendMode)
        {
            if (DressingMode == DressingModeEnum.error) return;
            Mat1st.SetFloat(PropNameDressBlendMode, blendMode);
        }

        public void Execute()
        {
            SetEnable();
            SetTexture(Target.Tex);
            SetBlendMode((float)Target.BlendMode);
        }

        public void CreateSample()
        {
            if (SampleObject == null)
            {
                GameObject duplicate = UnityEngine.Object.Instantiate(GameObject);
                duplicate.SetActive(true);
                duplicate.name = sampleObjName;
                duplicate.transform.SetParent(Target.transform);
            }
            Renderer rendererOrg = GameObject?.GetComponentInChildren<Renderer>();
            Renderer rendererSample = SampleObject?.GetComponentInChildren<Renderer>();
            Material[] materials = rendererOrg.sharedMaterials.Select(m => new Material(m)).ToArray();
            rendererSample.materials = materials;
            var SampleDressingTarget = new DressingTarget(Target, SampleObject);
            SampleDressingTarget.Execute();
            GameObject.SetActive(false);
        }

        public void RemoveSample()
        {
            if (GameObject != null) GameObject.SetActive(true);
            if (SampleObject == null) return;
            GameObject.DestroyImmediate(SampleObject);
        }

        private bool CheckProperty(string pnEnable, string pnTex, string pnBlendMode)
        {
            if(Mat1st.HasProperty(pnEnable) && Mat1st.HasProperty(pnTex) && Mat1st.HasProperty(pnBlendMode))
            {
                if(Mat1st.GetFloat(pnEnable) == 0f)
                {
                    PropNameDressEnable = pnEnable;
                    PropNameDressTex = pnTex;
                    PropNameDressBlendMode = pnBlendMode;
                    return true;
                }
            }
            else
            {
                PutError($@"シェーダープロパティの取得中に問題が発生しました　{pnEnable},{pnTex},{pnBlendMode}");
            }
            return false;
        }

        private void PutError(string msg)
        {
            ErrorMsg += $"{msg}\n\r";
        }
        private void PutWarning(string msg)
        {
            WarningMsg += $"{msg}\n\r";
        }
    }
}
#endif