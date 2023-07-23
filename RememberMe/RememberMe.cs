using System.Reflection;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using HarmonyLib;
using NeosModLoader;
using FrooxEngine;
using FrooxEngine.UIX;

namespace RememberMe
{
    public class RememberMe : NeosMod
    {
        public override string Name => "RememberMe";
        public override string Author => "AlexW-578";
        public override string Version => "0.1.0";
        public override string Link => "https://github.com/AlexW-578/RememberMe/";
        
        private static ModConfiguration Config;

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> Enabled =
            new ModConfigurationKey<bool>("Enabled", "Enable/Disable the Mod", () => true);

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> JustUsername =
            new ModConfigurationKey<bool>("Save just the Username", computeDefault: () => true);

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> AutoLogin =
            new ModConfigurationKey<bool>("Auto Logon", computeDefault: () => false);

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<string> Username =
            new ModConfigurationKey<string>("Username", computeDefault: () => "", internalAccessOnly: true);

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> UseSecretMachineID =
            new ModConfigurationKey<bool>("Use Secret Machine ID", "Use Secret Machine ID as encryption key",
                computeDefault: () => true);

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<string> EncryptionPassword =
            new ModConfigurationKey<string>("EncryptionPassword (Change Me!)",
                computeDefault: () => "D3C2xUTbMxPsgTfXD@E$#DqVLNarJf`svKgJyamXv3mZMXdPbF", internalAccessOnly: true);

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<string> EncryptedPassword =
            new ModConfigurationKey<string>("EncryptedPassword", computeDefault: () => "", internalAccessOnly: true);

        private static readonly MethodInfo _Login = AccessTools.Method(typeof(LoginDialog), "Login");

        public override void OnEngineInit()
        {
            Config = GetConfiguration();
            Config.Save(true);
            Harmony harmony = new Harmony("co.uk.AlexW-578.RememberMe");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(FrooxEngine.LoginDialog), "BuildUI")]
        class RememberMe_Dialog_Postfix_Patch
        {
            static void Postfix(SyncRef<TextField> ____username, SyncRef<TextField> ____password,
                SyncRef<Checkbox> ____rememberLogin, LoginDialog __instance)
            {
                Warn(__instance.Engine.LocalDB.SecretMachineID);
                if (Config.GetValue(Enabled))
                {
                    if (Config.GetValue(JustUsername))
                    {
                        ____username.Target.TargetString = Config.GetValue(Username);
                    }
                    else
                    {
                        ____username.Target.TargetString = Config.GetValue(Username);
                        if (!(Config.GetValue(EncryptedPassword) is null))
                        {
                            byte[] data = System.Convert.FromBase64String(Config.GetValue(EncryptedPassword));

                            byte[] decrypted = ProtectedData.Unprotect(data,
                                Encoding.Unicode.GetBytes(__instance.Engine.LocalDB.SecretMachineID),
                                DataProtectionScope.LocalMachine);
                            
                            if (!Config.GetValue(UseSecretMachineID))
                            {
                                decrypted = ProtectedData.Unprotect(data,
                                    Encoding.Unicode.GetBytes(Config.GetValue(EncryptionPassword)),
                                    DataProtectionScope.LocalMachine);
                            }

                            ____password.Target.TargetString = Encoding.Unicode.GetString(decrypted);

                            if (Config.GetValue(AutoLogin))
                            {
                                ____rememberLogin.Target.IsChecked = true;
                                var loginTask = (Task)_Login.Invoke(__instance,
                                    new object[]
                                    {
                                        ____username.Target.TargetString, Encoding.Unicode.GetString(decrypted), null,
                                        null
                                    });
                                Task.Run(async () => await loginTask);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(FrooxEngine.LoginDialog), "Login")]
        class RememberMe_Login_Prefix_Patch
        {
            static bool Prefix(string credential, string password,
                SyncRef<Checkbox> ____rememberLogin, LoginDialog __instance)
            {
                if (!Config.GetValue(Enabled))
                {
                    return true;
                }

                if (____rememberLogin.Target.IsChecked & Config.GetValue(JustUsername) &
                    !(credential is null))
                {
                    Config.Set(Username, credential);
                }

                if (____rememberLogin.Target.IsChecked & !Config.GetValue(JustUsername) & !(password is null) &
                    !(credential is null))
                {
                    Config.Set(Username, credential);
                    var data = Encoding.Unicode.GetBytes(password);
                    byte[] encrypted = System.Security.Cryptography.ProtectedData.Protect(data,
                        Encoding.Unicode.GetBytes(Config.GetValue(EncryptionPassword)),
                        DataProtectionScope.LocalMachine);
                    if (!Config.GetValue(UseSecretMachineID))
                    {
                        encrypted = System.Security.Cryptography.ProtectedData.Protect(data,
                            Encoding.Unicode.GetBytes(Config.GetValue(EncryptionPassword)),
                            DataProtectionScope.LocalMachine);
                    }


                    Config.Set(EncryptedPassword, System.Convert.ToBase64String(encrypted));
                }

                return true;
            }
        }
    }
}