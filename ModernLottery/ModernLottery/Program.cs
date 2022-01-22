using NBitcoin;
using System.Security.Cryptography;

namespace ModernLottery
{
    internal class Program
    {
        static int count = 0;
        static UnsecureRandom random = new UnsecureRandom();
        static HashSet<string> knownWallets = new();
        static Network bitcoinMain = NBitcoin.Network.Main;
        static Network bitcoinCash = NBitcoin.Altcoins.BCash.Instance.Mainnet;

        public static string ByteArrayToString(byte[] bytes)
        {
            if (bytes == null) return "null";
            string joinedBytes = string.Join(", ", bytes.Select(b => b.ToString()));
            return $"new byte[] {{ {joinedBytes} }}";
        }

        static void Main(string[] args)
        {
            foreach (var line in File.ReadLines("Bitcoin_addresses_LATEST.txt"))
            {
                if (!string.IsNullOrEmpty(line))
                {
                    knownWallets.Add(line);
                }
            }

            CheckWallet(new byte[32] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 });
            Console.WriteLine("Started");

            for (int i = 0; i < Environment.ProcessorCount - 2; i++)
            {
                new Thread(new ThreadStart(ThreadMain)).Start();
            }

            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine(count);
            }
        }

        static void ThreadMain()
        {
            while (true)
            {
                CheckWallet();
            }
        }

        static void CheckWallet(byte[] input = null)
        {
            ++count;

            var privateSeed = input ?? RandomNumberGenerator.GetBytes(32);
            var key = new Key(privateSeed); // private key
            var pair = key.CreateKeyPair(); // key pair (pub+priv keys)
            var addrP2pkh = key.GetAddress(ScriptPubKeyType.Legacy, bitcoinMain); // old type address
            var addrP2wpkh = key.GetAddress(ScriptPubKeyType.Segwit, bitcoinMain); // old type address
            var addrP2wpkt = key.GetAddress(ScriptPubKeyType.TaprootBIP86, bitcoinMain); // old type address
            var addrP2sh = key.GetAddress(ScriptPubKeyType.SegwitP2SH, bitcoinMain); // new type address

            if (knownWallets.Contains(addrP2pkh.ToString())
                || knownWallets.Contains(addrP2sh.ToString())
                || knownWallets.Contains(addrP2wpkh.ToString())
                || knownWallets.Contains(addrP2wpkt.ToString()))
            {
                if (input != null)
                {
                    Console.WriteLine("Test OK");
                    return;
                }

                File.AppendAllLines("Hacked.txt", new string[] { ByteArrayToString(privateSeed) });
                File.AppendAllLines($"Hacked{DateTime.Now.Ticks}.txt", new string[] { ByteArrayToString(privateSeed) });
                Console.WriteLine("----------------------");
                Console.WriteLine("HACKED LOOOL");
                Console.WriteLine(ByteArrayToString(privateSeed));
                Console.WriteLine("----------------------");
            }
        }
    }
}
