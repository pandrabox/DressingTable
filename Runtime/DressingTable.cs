#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using com.github.pandrabox.pandravase.runtime;
using static com.github.pandrabox.pandravase.runtime.Util;
using static com.github.pandrabox.pandravase.runtime.Global;
using static com.github.pandrabox.dressingtable.runtime.Global;
using System.Linq;

namespace com.github.pandrabox.dressingtable.runtime
{
    public class DressingTable : MonoBehaviour, IEditorOnly
    {
    }


    [CustomEditor(typeof(DressingTable))]
    public class DressingTableEditor : PandraEditor
    {
        public DressingTableEditor() : base(true, PROJECTNAME, ProjectTypes.Asset)
        {
        }


        protected override void DefineSerial()
        {
        }

        public override void OnInnerInspectorGUI()
        {
            Title("説明");
            EditorGUILayout.HelpBox("dressingtableは「CHILD WITCH」様の「PoseClipper」のインストールをサポートします。\n「PoseClipper」と、このプレハブをアバター直下に入れればインストールが完了します \n 動画で説明されている手動設定は不要です（やった場合こちらは使わないで下さい）", MessageType.Info);



            Title("チェック");
            var desc = GetAvatarDescriptor(((DressingTable)target).gameObject);
            //var map = new PoseClipMap(desc);

            //if (map.AnyError)
            //{
            //    EditorGUILayout.HelpBox($"エラーがあります。解決しないとアバターがアップロードできません。次の処置をご検討下さい。\n・エラー内容を手動で修正する\n・エラーを報告する\n・本Installerを削除する", MessageType.Error);
            //    EditorGUILayout.HelpBox($"{map.ErrMsg}", MessageType.Error);
            //}
            //else
            //{
            //    EditorGUILayout.HelpBox("dressingtableは適切に準備できています！", MessageType.Info);
            //}
        }
    }
}
#endif