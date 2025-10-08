using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Thirdweb;

namespace Thirdweb.Unity.Examples
{
    [System.Serializable]
    public class WalletPanelUI
    {
        public string Identifier;
        public GameObject Panel;
        public Button Action1Button;
        public Button Action2Button;
        public Button Action3Button;
        public Button BackButton;
        public Button NextButton;
        public TMP_Text LogText;
        public TMP_InputField InputField;
        public Button InputFieldSubmitButton;
    }

    public class PlaygroundManager : MonoBehaviour
    {
        [field: SerializeField, Header("Wallet Options")]
        private ulong ActiveChainId = 80002;

        [field: SerializeField]
        private bool WebglForceMetamaskExtension = false;

        [field: SerializeField, Header("Connect Wallet")]
        private GameObject ConnectWalletPanel;

        [field: SerializeField]
        private Button PrivateKeyWalletButton;

        [field: SerializeField]
        private Button EcosystemWalletButton;

        [field: SerializeField]
        private Button WalletConnectButton;

        [field: SerializeField, Header("Wallet Panels")]
        private List<WalletPanelUI> WalletPanels;

        private ThirdwebChainData _chainDetails;
        private string customContractAddress = "0x354ace99Ea67A4117196b8d976c245a1f329bDA2";
        //? 新版不用 private string abi = "[{\"inputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"studentId\",\"type\":\"uint256\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"examId\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"string\",\"name\":\"examName\",\"type\":\"string\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"bool\",\"name\":\"passed\",\"type\":\"bool\"}],\"name\":\"ExamRecorded\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"oldScore\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"newScore\",\"type\":\"uint256\"}],\"name\":\"PassingScoreUpdated\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"studentId\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"string\",\"name\":\"name\",\"type\":\"string\"},{\"indexed\":false,\"internalType\":\"string\",\"name\":\"email\",\"type\":\"string\"}],\"name\":\"StudentAdded\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"string\",\"name\":\"_name\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"_email\",\"type\":\"string\"},{\"internalType\":\"uint8\",\"name\":\"_age\",\"type\":\"uint8\"}],\"name\":\"addStudent\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"getAllStudentIds\",\"outputs\":[{\"internalType\":\"uint256[]\",\"name\":\"\",\"type\":\"uint256[]\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_studentId\",\"type\":\"uint256\"}],\"name\":\"getAverageScore\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"average\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"getContractStats\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"_totalStudents\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_totalExams\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_passingScore\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_studentId\",\"type\":\"uint256\"}],\"name\":\"getPassedExamsCount\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"passedCount\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_studentId\",\"type\":\"uint256\"}],\"name\":\"getStudent\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"id\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"name\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"email\",\"type\":\"string\"},{\"internalType\":\"uint8\",\"name\":\"age\",\"type\":\"uint8\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_studentId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_examIndex\",\"type\":\"uint256\"}],\"name\":\"getStudentExam\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"examId\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"examName\",\"type\":\"string\"},{\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxScore\",\"type\":\"uint256\"},{\"internalType\":\"bool\",\"name\":\"passed\",\"type\":\"bool\"},{\"internalType\":\"uint256\",\"name\":\"timestamp\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"remarks\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_studentId\",\"type\":\"uint256\"}],\"name\":\"getStudentExamCount\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_studentId\",\"type\":\"uint256\"}],\"name\":\"getStudentExams\",\"outputs\":[{\"components\":[{\"internalType\":\"uint256\",\"name\":\"examId\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"examName\",\"type\":\"string\"},{\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxScore\",\"type\":\"uint256\"},{\"internalType\":\"bool\",\"name\":\"passed\",\"type\":\"bool\"},{\"internalType\":\"uint256\",\"name\":\"timestamp\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"remarks\",\"type\":\"string\"}],\"internalType\":\"struct HoloContract.ExamResult[]\",\"name\":\"\",\"type\":\"tuple[]\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_studentId\",\"type\":\"uint256\"}],\"name\":\"isStudentExists\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"nextExamId\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"nextStudentId\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"owner\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"passingScore\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_studentId\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"_examName\",\"type\":\"string\"},{\"internalType\":\"uint256\",\"name\":\"_score\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_maxScore\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"_remarks\",\"type\":\"string\"}],\"name\":\"recordExam\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"studentExams\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"examId\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"examName\",\"type\":\"string\"},{\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxScore\",\"type\":\"uint256\"},{\"internalType\":\"bool\",\"name\":\"passed\",\"type\":\"bool\"},{\"internalType\":\"uint256\",\"name\":\"timestamp\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"remarks\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"students\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"id\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"name\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"email\",\"type\":\"string\"},{\"internalType\":\"uint8\",\"name\":\"age\",\"type\":\"uint8\"},{\"internalType\":\"bool\",\"name\":\"exists\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"totalStudents\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_newOwner\",\"type\":\"address\"}],\"name\":\"transferOwnership\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_newPassingScore\",\"type\":\"uint256\"}],\"name\":\"updatePassingScore\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_studentId\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"_name\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"_email\",\"type\":\"string\"},{\"internalType\":\"uint8\",\"name\":\"_age\",\"type\":\"uint8\"}],\"name\":\"updateStudent\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";

        private void Awake()
        {
            InitializePanels();
        }

        private async void Start()
        {
            try
            {
                _chainDetails = await Utils.GetChainMetadata(client: ThirdwebManager.Instance.Client, chainId: ActiveChainId);
            }
            catch
            {
                _chainDetails = new ThirdwebChainData()
                {
                    NativeCurrency = new ThirdwebChainNativeCurrency()
                    {
                        Decimals = 18,
                        Name = "ETH",
                        Symbol = "ETH"
                    }
                };
            }
        }

        private void InitializePanels()
        {
            CloseAllPanels();

            ConnectWalletPanel.SetActive(true);

            PrivateKeyWalletButton.onClick.RemoveAllListeners();
            PrivateKeyWalletButton.onClick.AddListener(() =>
            {
                var options = GetWalletOptions(WalletProvider.PrivateKeyWallet);
                ConnectWallet(options);
            });

            EcosystemWalletButton.onClick.RemoveAllListeners();
            EcosystemWalletButton.onClick.AddListener(() => InitializeEcosystemWalletPanel());

            WalletConnectButton.onClick.RemoveAllListeners();
            WalletConnectButton.onClick.AddListener(() =>
            {
                var options = GetWalletOptions(WalletProvider.WalletConnectWallet);
                ConnectWallet(options);
            });
        }

        private async void ConnectWallet(WalletOptions options)
        {
            // Connect the wallet

            var internalWalletProvider = options.Provider == WalletProvider.MetaMaskWallet ? WalletProvider.WalletConnectWallet : options.Provider;
            var currentPanel = WalletPanels.Find(panel => panel.Identifier == internalWalletProvider.ToString());

            Log(currentPanel.LogText, $"Connecting...");

            var wallet = await ThirdwebManager.Instance.ConnectWallet(options);

            // Initialize the wallet panel

            CloseAllPanels();

            // Setup actions

            ClearLog(currentPanel.LogText);
            currentPanel.Panel.SetActive(true);

            currentPanel.BackButton.onClick.RemoveAllListeners();
            currentPanel.BackButton.onClick.AddListener(InitializePanels);

            currentPanel.NextButton.onClick.RemoveAllListeners();
            currentPanel.NextButton.onClick.AddListener(InitializeContractsPanel);

            currentPanel.Action1Button.onClick.RemoveAllListeners();
            currentPanel.Action1Button.onClick.AddListener(async () =>
            {
                // *原本的方法
                // var address = await wallet.GetAddress();
                // address.CopyToClipboard();
                // Log(currentPanel.LogText, $"Address: {address}");
                try
                {
                    LoadingLog(currentPanel.LogText);

                    // 設定要查詢的學生 ID
                    uint studentId = 4;

                    var contract = await ThirdwebManager.Instance.GetContract(address: customContractAddress, chainId: ActiveChainId);
                    Log(currentPanel.LogText, $"Contract loaded successfully");

                    // 呼叫getStudent 方法
                    var result = await contract.Read<object[]>("getStudent", studentId);
                    Log(currentPanel.LogText, $"Contract call completed");

                    // 檢查回傳結果
                    if (result != null && result.Length >= 4)
                    {
                        string id = result[0]?.ToString() ?? "N/A";
                        string name = result[1]?.ToString() ?? "N/A";
                        string email = result[2]?.ToString() ?? "N/A";
                        string age = result[3]?.ToString() ?? "N/A";

                        string displayText = $"Student found!\nID: {id}\nName: {name}\nEmail: {email}\nAge: {age}";
                        Log(currentPanel.LogText, displayText);

                    }
                    else
                    {
                        Log(currentPanel.LogText, $"Invalid response: result is null or has insufficient data. Length: {result?.Length ?? 0}");
                    }
                }
                catch (System.Exception e)
                {
                    Log(currentPanel.LogText, $"Error: {e.Message}");
                    Debug.LogError($"Full error details: {e}");
                }
            });

            currentPanel.Action2Button.onClick.RemoveAllListeners();
            currentPanel.Action2Button.onClick.AddListener(async () =>
            {
                var message = "Hello World!";
                var signature = await wallet.PersonalSign(message);
                Log(currentPanel.LogText, $"Signature: {signature}");
            });

            currentPanel.Action3Button.onClick.RemoveAllListeners();
            currentPanel.Action3Button.onClick.AddListener(async () =>
            {
                LoadingLog(currentPanel.LogText);
                var balance = await wallet.GetBalance(chainId: ActiveChainId);
                var balanceEth = Utils.ToEth(wei: balance.ToString(), decimalsToDisplay: 4, addCommas: true);
                Log(currentPanel.LogText, $"Balance: {balanceEth} {_chainDetails.NativeCurrency.Symbol}");
            });
        }

        private WalletOptions GetWalletOptions(WalletProvider provider)
        {
            switch (provider)
            {
                case WalletProvider.PrivateKeyWallet:
                    return new WalletOptions(provider: WalletProvider.PrivateKeyWallet, chainId: ActiveChainId);
                case WalletProvider.EcosystemWallet:
                    var ecosystemWalletOptions = new EcosystemWalletOptions(ecosystemId: "ecosystem.the-bonfire", authprovider: AuthProvider.Google);
                    return new WalletOptions(provider: WalletProvider.EcosystemWallet, chainId: ActiveChainId, ecosystemWalletOptions: ecosystemWalletOptions);
                case WalletProvider.WalletConnectWallet:
                    var externalWalletProvider =
                        Application.platform == RuntimePlatform.WebGLPlayer && WebglForceMetamaskExtension ? WalletProvider.MetaMaskWallet : WalletProvider.WalletConnectWallet;
                    return new WalletOptions(provider: externalWalletProvider, chainId: ActiveChainId);
                default:
                    throw new System.NotImplementedException("Wallet provider not implemented for this example.");
            }
        }

        private void InitializeEcosystemWalletPanel()
        {
            var panel = WalletPanels.Find(walletPanel => walletPanel.Identifier == "EcosystemWallet_Authentication");

            CloseAllPanels();

            ClearLog(panel.LogText);
            panel.Panel.SetActive(true);

            panel.BackButton.onClick.RemoveAllListeners();
            panel.BackButton.onClick.AddListener(InitializePanels);

            // Email
            panel.Action1Button.onClick.RemoveAllListeners();
            panel.Action1Button.onClick.AddListener(() =>
            {
                InitializeEcosystemWalletPanel_Email();
            });

            // Phone
            panel.Action2Button.onClick.RemoveAllListeners();
            panel.Action2Button.onClick.AddListener(() =>
            {
                InitializeEcosystemWalletPanel_Phone();
            });

            // Socials
            panel.Action3Button.onClick.RemoveAllListeners();
            panel.Action3Button.onClick.AddListener(() =>
            {
                InitializeEcosystemWalletPanel_Socials();
            });
        }

        private void InitializeEcosystemWalletPanel_Email()
        {
            var panel = WalletPanels.Find(walletPanel => walletPanel.Identifier == "EcosystemWallet_Email");

            CloseAllPanels();

            ClearLog(panel.LogText);
            panel.Panel.SetActive(true);

            panel.BackButton.onClick.RemoveAllListeners();
            panel.BackButton.onClick.AddListener(InitializeEcosystemWalletPanel);

            panel.InputFieldSubmitButton.onClick.RemoveAllListeners();
            panel.InputFieldSubmitButton.onClick.AddListener(() =>
            {
                try
                {
                    var email = panel.InputField.text;
                    var ecosystemWalletOptions = new EcosystemWalletOptions(ecosystemId: "ecosystem.the-bonfire", email: email);
                    var options = new WalletOptions(provider: WalletProvider.EcosystemWallet, chainId: ActiveChainId, ecosystemWalletOptions: ecosystemWalletOptions);
                    ConnectWallet(options);
                }
                catch (System.Exception e)
                {
                    Log(panel.LogText, e.Message);
                }
            });
        }

        private void InitializeEcosystemWalletPanel_Phone()
        {
            var panel = WalletPanels.Find(walletPanel => walletPanel.Identifier == "EcosystemWallet_Phone");

            CloseAllPanels();

            ClearLog(panel.LogText);
            panel.Panel.SetActive(true);

            panel.BackButton.onClick.RemoveAllListeners();
            panel.BackButton.onClick.AddListener(InitializeEcosystemWalletPanel);

            panel.InputFieldSubmitButton.onClick.RemoveAllListeners();
            panel.InputFieldSubmitButton.onClick.AddListener(() =>
            {
                try
                {
                    var phone = panel.InputField.text;
                    var ecosystemWalletOptions = new EcosystemWalletOptions(ecosystemId: "ecosystem.the-bonfire", phoneNumber: phone);
                    var options = new WalletOptions(provider: WalletProvider.EcosystemWallet, chainId: ActiveChainId, ecosystemWalletOptions: ecosystemWalletOptions);
                    ConnectWallet(options);
                }
                catch (System.Exception e)
                {
                    Log(panel.LogText, e.Message);
                }
            });
        }

        private void InitializeEcosystemWalletPanel_Socials()
        {
            var panel = WalletPanels.Find(walletPanel => walletPanel.Identifier == "EcosystemWallet_Socials");

            CloseAllPanels();

            ClearLog(panel.LogText);
            panel.Panel.SetActive(true);

            panel.BackButton.onClick.RemoveAllListeners();
            panel.BackButton.onClick.AddListener(InitializeEcosystemWalletPanel);

            // socials action 1 is google, 2 is apple 3 is discord

            panel.Action1Button.onClick.RemoveAllListeners();
            panel.Action1Button.onClick.AddListener(() =>
            {
                try
                {
                    Log(panel.LogText, "Authenticating...");
                    var ecosystemWalletOptions = new EcosystemWalletOptions(ecosystemId: "ecosystem.the-bonfire", authprovider: AuthProvider.Google);
                    var options = new WalletOptions(provider: WalletProvider.EcosystemWallet, chainId: ActiveChainId, ecosystemWalletOptions: ecosystemWalletOptions);
                    ConnectWallet(options);
                }
                catch (System.Exception e)
                {
                    Log(panel.LogText, e.Message);
                }
            });

            panel.Action2Button.onClick.RemoveAllListeners();
            panel.Action2Button.onClick.AddListener(() =>
            {
                try
                {
                    Log(panel.LogText, "Authenticating...");
                    var ecosystemWalletOptions = new EcosystemWalletOptions(ecosystemId: "ecosystem.the-bonfire", authprovider: AuthProvider.Apple);
                    var options = new WalletOptions(provider: WalletProvider.EcosystemWallet, chainId: ActiveChainId, ecosystemWalletOptions: ecosystemWalletOptions);
                    ConnectWallet(options);
                }
                catch (System.Exception e)
                {
                    Log(panel.LogText, e.Message);
                }
            });

            panel.Action3Button.onClick.RemoveAllListeners();
            panel.Action3Button.onClick.AddListener(() =>
            {
                try
                {
                    Log(panel.LogText, "Authenticating...");
                    var ecosystemWalletOptions = new EcosystemWalletOptions(ecosystemId: "ecosystem.the-bonfire", authprovider: AuthProvider.Discord);
                    var options = new WalletOptions(provider: WalletProvider.EcosystemWallet, chainId: ActiveChainId, ecosystemWalletOptions: ecosystemWalletOptions);
                    ConnectWallet(options);
                }
                catch (System.Exception e)
                {
                    Log(panel.LogText, e.Message);
                }
            });
        }

        private void InitializeContractsPanel()
        {
            var panel = WalletPanels.Find(walletPanel => walletPanel.Identifier == "Contracts");

            CloseAllPanels();

            ClearLog(panel.LogText);
            panel.Panel.SetActive(true);

            panel.BackButton.onClick.RemoveAllListeners();
            panel.BackButton.onClick.AddListener(InitializePanels);

            panel.NextButton.onClick.RemoveAllListeners();
            panel.NextButton.onClick.AddListener(InitializeAccountAbstractionPanel);

            // Get NFT
            panel.Action1Button.onClick.RemoveAllListeners();
            panel.Action1Button.onClick.AddListener(async () =>
            {
                try
                {
                    LoadingLog(panel.LogText);
                    var dropErc1155Contract = await ThirdwebManager.Instance.GetContract(address: "0x94894F65d93eb124839C667Fc04F97723e5C4544", chainId: ActiveChainId);
                    var nft = await dropErc1155Contract.ERC1155_GetNFT(tokenId: 1);
                    Log(panel.LogText, $"NFT: {JsonConvert.SerializeObject(nft.Metadata)}");
                    var sprite = await nft.GetNFTSprite(client: ThirdwebManager.Instance.Client);
                    // spawn image for 3s
                    var image = new GameObject("NFT Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    image.transform.SetParent(panel.Panel.transform, false);
                    image.GetComponent<Image>().sprite = sprite;
                    Destroy(image, 3f);
                }
                catch (System.Exception e)
                {
                    Log(panel.LogText, e.Message);
                }
            });

            // Call contract
            panel.Action2Button.onClick.RemoveAllListeners();
            panel.Action2Button.onClick.AddListener(async () =>
            {
                try
                {
                    LoadingLog(panel.LogText);
                    var contract = await ThirdwebManager.Instance.GetContract(address: "0x6A7a26c9a595E6893C255C9dF0b593e77518e0c3", chainId: ActiveChainId);
                    var result = await contract.ERC1155_URI(tokenId: 1);
                    Log(panel.LogText, $"Result (uri): {result}");
                }
                catch (System.Exception e)
                {
                    Log(panel.LogText, e.Message);
                }
            });

            // Get ERC20 Balance
            panel.Action3Button.onClick.RemoveAllListeners();
            panel.Action3Button.onClick.AddListener(async () =>
            {
                try
                {
                    LoadingLog(panel.LogText);
                    var dropErc20Contract = await ThirdwebManager.Instance.GetContract(address: "0xEBB8a39D865465F289fa349A67B3391d8f910da9", chainId: ActiveChainId);
                    var symbol = await dropErc20Contract.ERC20_Symbol();
                    var balance = await dropErc20Contract.ERC20_BalanceOf(ownerAddress: await ThirdwebManager.Instance.GetActiveWallet().GetAddress());
                    var balanceEth = Utils.ToEth(wei: balance.ToString(), decimalsToDisplay: 0, addCommas: false);
                    Log(panel.LogText, $"Balance: {balanceEth} {symbol}");
                }
                catch (System.Exception e)
                {
                    Log(panel.LogText, e.Message);
                }
            });
        }
        public void InitializeTestPanel()
        {
            var panel = WalletPanels.Find(walletPanel => walletPanel.Identifier == "TestPanel");

            CloseAllPanels();

            ClearLog(panel.LogText);
            panel.Panel.SetActive(true);

            panel.BackButton.onClick.RemoveAllListeners();
            panel.BackButton.onClick.AddListener(InitializePanels);

            panel.NextButton.onClick.RemoveAllListeners();
            panel.NextButton.onClick.AddListener(InitializeAccountAbstractionPanel);

            // Get NFT
            panel.Action1Button.onClick.RemoveAllListeners();
            panel.Action1Button.onClick.AddListener(async () =>
            {
                try
                {
                    LoadingLog(panel.LogText);
                    uint studentId = 4;
                    var contract = await ThirdwebManager.Instance.GetContract(address: customContractAddress, chainId: ActiveChainId);
                    
                    var result = await contract.Read<object[]>("getStudent", studentId);
                    
                    if (result != null && result.Length >= 4)
                    {
                        uint id = System.Convert.ToUInt32(result[0]);
                        string name = result[1].ToString();
                        string email = result[2].ToString();
                        byte age = System.Convert.ToByte(result[3]);
                        
                        string displayText = $"ID: {id}\nName: {name}\nEmail: {email}\nAge: {age}";
                        Log(panel.LogText, displayText);
                    }
                    else
                    {
                        Log(panel.LogText, "No student data found or invalid response format");
                    }
                }
                catch (System.Exception e)
                {
                    Log(panel.LogText, $"Error getting student: {e.Message}");
                    Debug.LogError($"Error getting student: {e.Message}");
                }
            });

            // Call contract
            panel.Action2Button.onClick.RemoveAllListeners();
            panel.Action2Button.onClick.AddListener(async () =>
            {
                try
                {
                    LoadingLog(panel.LogText);
                    var contract = await ThirdwebManager.Instance.GetContract(address: "0x6A7a26c9a595E6893C255C9dF0b593e77518e0c3", chainId: ActiveChainId);
                    var result = await contract.ERC1155_URI(tokenId: 1);
                    Log(panel.LogText, $"Result (uri): {result}");
                }
                catch (System.Exception e)
                {
                    Log(panel.LogText, e.Message);
                }
            });

            // Get ERC20 Balance
            panel.Action3Button.onClick.RemoveAllListeners();
            panel.Action3Button.onClick.AddListener(async () =>
            {
                try
                {
                    LoadingLog(panel.LogText);
                    var dropErc20Contract = await ThirdwebManager.Instance.GetContract(address: "0xEBB8a39D865465F289fa349A67B3391d8f910da9", chainId: ActiveChainId);
                    var symbol = await dropErc20Contract.ERC20_Symbol();
                    var balance = await dropErc20Contract.ERC20_BalanceOf(ownerAddress: await ThirdwebManager.Instance.GetActiveWallet().GetAddress());
                    var balanceEth = Utils.ToEth(wei: balance.ToString(), decimalsToDisplay: 0, addCommas: false);
                    Log(panel.LogText, $"Balance: {balanceEth} {symbol}");
                }
                catch (System.Exception e)
                {
                    Log(panel.LogText, e.Message);
                }
            });
        }

        private async void InitializeAccountAbstractionPanel()
        {
            var currentWallet = ThirdwebManager.Instance.GetActiveWallet();
            var smartWallet = await ThirdwebManager.Instance.UpgradeToSmartWallet(personalWallet: currentWallet, chainId: ActiveChainId, smartWalletOptions: new SmartWalletOptions(sponsorGas: true));

            var panel = WalletPanels.Find(walletPanel => walletPanel.Identifier == "AccountAbstraction");

            CloseAllPanels();

            ClearLog(panel.LogText);
            panel.Panel.SetActive(true);

            panel.BackButton.onClick.RemoveAllListeners();
            panel.BackButton.onClick.AddListener(InitializePanels);

            // Personal Sign (1271)
            panel.Action1Button.onClick.RemoveAllListeners();
            panel.Action1Button.onClick.AddListener(async () =>
            {
                try
                {
                    var message = "Hello, World!";
                    var signature = await smartWallet.PersonalSign(message);
                    Log(panel.LogText, $"Signature: {signature}");
                }
                catch (System.Exception e)
                {
                    Log(panel.LogText, e.Message);
                }
            });

            // Create Session Key
            panel.Action2Button.onClick.RemoveAllListeners();
            panel.Action2Button.onClick.AddListener(async () =>
            {
                try
                {
                    Log(panel.LogText, "Granting Session Key...");
                    var randomWallet = await PrivateKeyWallet.Generate(ThirdwebManager.Instance.Client);
                    var randomWalletAddress = await randomWallet.GetAddress();
                    var timeTomorrow = Utils.GetUnixTimeStampNow() + 60 * 60 * 24;
                    var sessionKey = await smartWallet.CreateSessionKey(
                        signerAddress: randomWalletAddress,
                        approvedTargets: new List<string> { Constants.ADDRESS_ZERO },
                        nativeTokenLimitPerTransactionInWei: "0",
                        permissionStartTimestamp: "0",
                        permissionEndTimestamp: timeTomorrow.ToString(),
                        reqValidityStartTimestamp: "0",
                        reqValidityEndTimestamp: timeTomorrow.ToString()
                    );
                    Log(panel.LogText, $"Session Key Created for {randomWalletAddress}: {sessionKey.TransactionHash}");
                }
                catch (System.Exception e)
                {
                    Log(panel.LogText, e.Message);
                }
            });

            // Get Active Signers
            panel.Action3Button.onClick.RemoveAllListeners();
            panel.Action3Button.onClick.AddListener(async () =>
            {
                try
                {
                    LoadingLog(panel.LogText);
                    var activeSigners = await smartWallet.GetAllActiveSigners();
                    Log(panel.LogText, $"Active Signers: {JsonConvert.SerializeObject(activeSigners)}");
                }
                catch (System.Exception e)
                {
                    Log(panel.LogText, e.Message);
                }
            });
        }

        private void CloseAllPanels()
        {
            ConnectWalletPanel.SetActive(false);
            foreach (var walletPanel in WalletPanels)
            {
                walletPanel.Panel.SetActive(false);
            }
        }

        private void ClearLog(TMP_Text logText)
        {
            logText.text = string.Empty;
        }

        private void Log(TMP_Text logText, string message)
        {
            logText.text = message;
            ThirdwebDebug.Log(message);
        }

        private void LoadingLog(TMP_Text logText)
        {
            logText.text = "Loading...";
        }
    }
}
