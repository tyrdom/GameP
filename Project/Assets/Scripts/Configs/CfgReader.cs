using cfg;
using UnityEngine;
using Luban;
using Unity.VisualScripting;

namespace Configs
{
    public static class CfgReader
    {
        private static ByteBuf LoadByteBuf(string file)
        {
            var textAsset = Resources.Load<TextAsset>($"ConfigsExcel/{file}");
            if (textAsset == null)
            {
                Debug.LogError($"CfgReader: {file}.bytes not found");
                return null;
            }

            var textAssetBytes = textAsset.bytes;
            return new ByteBuf(textAssetBytes);
        }

        public static Tables GetTables()
        {
            var tables = new Tables(LoadByteBuf);
            return tables;
        }
    }
}