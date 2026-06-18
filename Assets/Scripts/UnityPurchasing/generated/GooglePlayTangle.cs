// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("HWKVMuBRGcOCC/En1SkKGdq9YzkKUxOs8ryzs0OJ/beKtg5T8YRr717sb0xeY2hnROgm6Jljb29va25to8x+a5bfCUo2B1+oStPlHHWpOhdbZU3BAz/soyfqqYHvV9OWAY+QciEWiIUcyUByeMuezNlRCbez4PdqlqSL0UwUXzy0Btahhoj6CTA72I7sb2FuXuxvZGzsb29utwhSzsTzfJZu+YEtWl7tSWWTyv0kwOJxWoozW18C+jw/Zzbn9ZMbyMuW0Q4MQ9SPaoth1LCd1MzFngY5ZKFy8g4pDxH73ZiLx1R3TYvzcQKBgmNwNtthT7ydPKgRZCzQHuiG6vRcXkv2mJ7IGR85RQkxoINwQOtJopob4CCvOfaCPDsuMUJAEWxtb25v");
        private static int[] order = new int[] { 6,8,6,10,11,9,12,8,13,13,12,13,12,13,14 };
        private static int key = 110;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
