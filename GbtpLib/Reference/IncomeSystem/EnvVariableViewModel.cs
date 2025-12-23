using IncomeSystem.Design.Common;
using IncomeSystem.Design.Popup;
using IncomeSystem.Library;
using MsSqlProcessor.MintechSql;
using MsSqlProcessor.MsSql;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GbtpLib.Logging;

namespace IncomeSystem.Design.EnvVariable
{
    public class EnvVariableViewModel : BindableBase
    {
        private static EnvVariableViewModel _instance;
        public static EnvVariableViewModel Instance
        {
            get => _instance ?? (_instance = new EnvVariableViewModel());
        }

        public class IntSystemInfoBinding : BindableBase
        {
            private string _intSystemHost = string.Empty;
            public string IntSystemHost
            {
                get => _intSystemHost;
                set
                {
                    SetProperty(ref _intSystemHost, value);
                }
            }

            private string _intSystemPort = string.Empty;
            public string IntSystemPort
            {
                get => _intSystemPort;
                set
                {
                    SetProperty(ref _intSystemPort, value);
                }
            }
        }

        public class MdzInfoBinding : BindableBase
        {
            private string _mdzIpAddress = string.Empty;
            public string MdzIpAddress
            {
                get => _mdzIpAddress;
                set
                {
                    SetProperty(ref _mdzIpAddress, value);
                }
            }

            private string _mdzPort = string.Empty;
            public string MdzPort
            {
                get => _mdzPort;
                set
                {
                    SetProperty(ref _mdzPort, value);
                }
            }
        }

        public class PositionInfoBinding : BindableBase
        {
            private List<string> _nameList = new List<string>();
            public List<string> NameList
            {
                get => _nameList;
                set
                {
                    SetProperty(ref _nameList, value);
                }
            }

            private string _name = string.Empty;
            public string Name
            {
                get => _name;
                set
                {
                    SetProperty(ref _name, value);
                }
            }

            private List<string> _codeList = new List<string>();
            public List<string> CodeList
            {
                get => _codeList;
                set
                {
                    SetProperty(ref _codeList, value);
                }
            }

            private string _code = string.Empty;
            public string Code
            {
                get => _code;
                set
                {
                    SetProperty(ref _code, value);
                }
            }
        }

        public class SimulationResultBinding : BindableBase
        {
            private List<string> _nameList = new List<string>();
            public List<string> NameList
            {
                get => _nameList;
                set
                {
                    SetProperty(ref _nameList, value);
                }
            }

            private string _name = string.Empty;
            public string Name
            {
                get => _name;
                set
                {
                    SetProperty(ref _name, value);
                }
            }
        }

        public PositionInfoBinding SiteInfo { get; set; } = new PositionInfoBinding();
        public PositionInfoBinding FactoryInfo { get; set; } = new PositionInfoBinding();
        public PositionInfoBinding WarehouseInfo { get; set; } = new PositionInfoBinding();
        public PositionInfoBinding ProcessInfo { get; set; } = new PositionInfoBinding();
        public PositionInfoBinding MachineInfo { get; set; } = new PositionInfoBinding();
        public PositionInfoBinding InspKindGroupInfo { get; set; } = new PositionInfoBinding();
        public PositionInfoBinding InspKindInfo { get; set; } = new PositionInfoBinding();
        public MdzInfoBinding MdzInfo { get; set; } = new MdzInfoBinding();
        public IntSystemInfoBinding IntSystemInfo { get; set; } = new IntSystemInfoBinding();
        public SimulationResultBinding SimulationResult { get; set; } = new SimulationResultBinding();

        public string _externalErrorDescription = string.Empty;
        public string ExternalErrorDescription
        {
            get => _externalErrorDescription;
            set => SetProperty(ref _externalErrorDescription, value);
        }

        public string _codeErrorDescription = string.Empty;
        public string CodeErrorDescription
        {
            get => _codeErrorDescription;
            set => SetProperty(ref _codeErrorDescription, value);
        }

        private AllBatterySlotViewModel _allBatterySlotViewModel = null;

        public EnvVariableViewModel()
        {
            try
            {
                InitializeCodeComboBox();
            }
            catch (Exception ex)
            {
                AppLog.Error("InitializeCodeComboBox in constructor failed.", ex);
            }
        }

        public void Loaded()
        {
            try
            {
                _allBatterySlotViewModel = AllBatterySlotViewModel.Instance;

                InitializeExternalData();
                InitializeCodeData();
            }
            catch (Exception ex)
            {
                AppLog.Error("EnvVariableViewModel.Loaded failed.", ex);
            }
        }

        private void InitializeExternalData()
        {
            try
            {
                var envVariable = LibEnvVariable.GetEnvVariable();

                MdzInfo.MdzIpAddress = envVariable.MDZ_IP_ADDR;
                MdzInfo.MdzPort = envVariable.MDZ_PORT;
                IntSystemInfo.IntSystemHost = envVariable.INT_SYSTEM_HOST;
                IntSystemInfo.IntSystemPort = envVariable.INT_SYSTEM_PORT;
            }
            catch (Exception ex)
            {
                AppLog.Error("InitializeExternalData failed.", ex);
            }
        }

        public void CancelExternalSetting()
        {
            try
            {
                ExternalErrorDescription = string.Empty;

                InitializeExternalData();
            }
            catch (Exception ex)
            {
                AppLog.Error("CancelExternalSetting failed.", ex);
            }
        }

        public void SaveExternalSetting()
        {
            try
            {
                ExternalErrorDescription = string.Empty;

                bool result = SaveExternalData();
                if (!result) return;

                // 데이터 베이스 연결 스트링 업데이트
                MLibMssql.SetConnectionString(
                    IntSystemInfo.IntSystemHost, IntSystemInfo.IntSystemPort,
                    "BRM_GBTP", "BRM_GBTP", "BRM_GBTP");
                MssqlBasic.SetConnectionString(
                    IntSystemInfo.IntSystemHost, IntSystemInfo.IntSystemPort,
                    "BRM_GBTP", "BRM_GBTP", "BRM_GBTP");

                ThreadStart thread = new ThreadStart(() =>
                {
                    try
                    {
                        InitializeCodeComboBox();
                        InitializeCodeData();

                        _allBatterySlotViewModel.Initialize();

                        if (SiteInfo.NameList.Count == 0)
                        {
                            new PopupWarningViewModel()
                            {
                                Title = "통합 시스템 DB 연결 실패",
                                Message = "통합 시스템 DB 연결이 실패하였습니다.",
                                CancelButtonVisibility = Visibility.Collapsed,
                            }.Open();
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLog.Error("SaveExternalSetting background initialization failed.", ex);
                    }
                });
                var popupWaitViewModel = new PopupWaitViewModel()
                {
                    Title = "코드 정보 초기화 중",
                }.Open(thread);
            }
            catch (Exception ex)
            {
                AppLog.Error("SaveExternalSetting failed.", ex);
            }
        }

        private bool SaveExternalData()
        {
            try
            {
                if (!IsExternalDataInputValid())
                {
                    return false;
                }

                // 팝업
                bool result = new PopupInfoViewModel()
                {
                    Title = "외부 설정 값 저장",
                    Message = "설정 값을 저장하시겠습니까?",
                    CancelButtonVisibility = Visibility.Visible,
                }.Open();

                if (result)
                {
                    EnvVariableData envVariable = new EnvVariableData
                    {
                        MDZ_IP_ADDR = MdzInfo.MdzIpAddress,
                        MDZ_PORT = MdzInfo.MdzPort,
                        INT_SYSTEM_HOST = IntSystemInfo.IntSystemHost,
                        INT_SYSTEM_PORT = IntSystemInfo.IntSystemPort,
                    };
                    LibEnvVariable.UpdateExternalEnvVariable(envVariable);

                    AppLog.Trace($"External settings saved: MDZ_IP_ADDR={envVariable.MDZ_IP_ADDR}, MDZ_PORT={envVariable.MDZ_PORT}, INT_SYSTEM_HOST={envVariable.INT_SYSTEM_HOST}, INT_SYSTEM_PORT={envVariable.INT_SYSTEM_PORT}");

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                AppLog.Error("SaveExternalData failed.", ex);
                return false;
            }
        }

        private bool IsExternalDataInputValid()
        {
            if (MdzInfo.MdzIpAddress == string.Empty)
            {
                ExternalErrorDescription = "MDZ IP를 입력해 주세요.";
                return false;
            }
            string pattern = @"^(25[0-5]|2[0-4][0-9]|1?[0-9]{1,2})\." +
                @"(25[0-5]|2[0-4][0-9]|1?[0-9]{1,2})\." +
                @"(25[0-5]|2[0-4][0-9]|1?[0-9]{1,2})\." +
                @"(25[0-5]|2[0-4][0-9]|1?[0-9]{1,2})$";
            Regex regex = new Regex(pattern);
            if (!regex.IsMatch(MdzInfo.MdzIpAddress))
            {
                ExternalErrorDescription = "MDZ IP 형식이 올바르지 않습니다.";
                return false;
            }
            if (MdzInfo.MdzPort == string.Empty)
            {
                ExternalErrorDescription = "MDZ PORT를 입력해 주세요.";
                return false;
            }
            if (IntSystemInfo.IntSystemHost == string.Empty)
            {
                ExternalErrorDescription = "통합 시스템 HOST를 입력해 주세요.";
                return false;
            }
            if (IntSystemInfo.IntSystemPort == string.Empty)
            {
                ExternalErrorDescription = "통합 시스템 PORT를 입력해 주세요.";
                return false;
            }

            return true;
        }

        private void InitializeCodeComboBox()
        {
            try
            {
                var mssql = new MLibMssql();
                mssql.Query(nameof(MST_SITE));
                mssql.Select<MST_SITE>(nameof(MST_SITE.SITE_CD));
                mssql.Select<MST_SITE>(nameof(MST_SITE.SITE_NM));
                mssql.Where(nameof(MST_SITE), nameof(MST_SITE.USE_YN), EYN_FLAG.Y);
                var mstSite = mssql.Get<MST_SITE>();
                SiteInfo.CodeList = mstSite.Select(x => x.SITE_CD).ToList();
                SiteInfo.NameList = mstSite.Select(x => x.SITE_NM).ToList();
                SiteInfo.Name = string.Empty;

                mssql.Query(nameof(MST_FACTORY));
                mssql.Select<MST_FACTORY>(nameof(MST_FACTORY.FACT_CD));
                mssql.Select<MST_FACTORY>(nameof(MST_FACTORY.FACT_NM));
                mssql.Where(nameof(MST_FACTORY), nameof(MST_FACTORY.USE_YN), EYN_FLAG.Y);
                var mstFactory = mssql.Get<MST_FACTORY>();
                FactoryInfo.CodeList = mstFactory.Select(x => x.FACT_CD).ToList();
                FactoryInfo.NameList = mstFactory.Select(x => x.FACT_NM).ToList();
                FactoryInfo.Name = string.Empty;

                mssql.Query(nameof(MST_WAREHOUSE));
                mssql.Select<MST_WAREHOUSE>(nameof(MST_WAREHOUSE.WH_CD));
                mssql.Select<MST_WAREHOUSE>(nameof(MST_WAREHOUSE.WH_NM));
                mssql.Where(nameof(MST_WAREHOUSE), nameof(MST_WAREHOUSE.USE_YN), EYN_FLAG.Y);
                var mstWarehouse = mssql.Get<MST_WAREHOUSE>();
                WarehouseInfo.CodeList = mstWarehouse.Select(x => x.WH_CD).ToList();
                WarehouseInfo.NameList = mstWarehouse.Select(x => x.WH_NM).ToList();
                WarehouseInfo.Name = string.Empty;

                mssql.Query(nameof(MST_PROCESS));
                mssql.Select<MST_PROCESS>(nameof(MST_PROCESS.PRCS_CD));
                mssql.Select<MST_PROCESS>(nameof(MST_PROCESS.PRCS_NM));
                mssql.Where(nameof(MST_PROCESS), nameof(MST_PROCESS.USE_YN), EYN_FLAG.Y);
                var mstProcess = mssql.Get<MST_PROCESS>();
                ProcessInfo.CodeList = mstProcess.Select(x => x.PRCS_CD).ToList();
                ProcessInfo.NameList = mstProcess.Select(x => x.PRCS_NM).ToList();
                ProcessInfo.Name = string.Empty;

                mssql.Query(nameof(MST_MACHINE));
                mssql.Select<MST_MACHINE>(nameof(MST_MACHINE.MC_CD));
                mssql.Select<MST_MACHINE>(nameof(MST_MACHINE.MC_NM));
                mssql.Where(nameof(MST_MACHINE), nameof(MST_MACHINE.USE_YN), EYN_FLAG.Y);
                var mstMachine = mssql.Get<MST_MACHINE>();
                MachineInfo.CodeList = mstMachine.Select(x => x.MC_CD).ToList();
                MachineInfo.NameList = mstMachine.Select(x => x.MC_NM).ToList();
                MachineInfo.Name = string.Empty;

                mssql.Query(nameof(MST_INSP_KIND_GROUP));
                mssql.Select<MST_INSP_KIND_GROUP>(nameof(MST_INSP_KIND_GROUP.INSP_KIND_GROUP_CD));
                mssql.Select<MST_INSP_KIND_GROUP>(nameof(MST_INSP_KIND_GROUP.INSP_KIND_GROUP_NM));
                mssql.Where(nameof(MST_INSP_KIND_GROUP), nameof(MST_INSP_KIND_GROUP.USE_YN), EYN_FLAG.Y);
                var mstInspKindGroup = mssql.Get<MST_INSP_KIND_GROUP>();
                InspKindGroupInfo.CodeList = mstInspKindGroup.Select(x => x.INSP_KIND_GROUP_CD).ToList();
                InspKindGroupInfo.NameList = mstInspKindGroup.Select(x => x.INSP_KIND_GROUP_NM).ToList();
                InspKindGroupInfo.Name = string.Empty;

                mssql.Query(nameof(MST_INSP_KIND));
                mssql.Select<MST_INSP_KIND>(nameof(MST_INSP_KIND.INSP_KIND_CD));
                mssql.Select<MST_INSP_KIND>(nameof(MST_INSP_KIND.INSP_KIND_NM));
                mssql.Where(nameof(MST_INSP_KIND), nameof(MST_INSP_KIND.USE_YN), EYN_FLAG.Y);
                var mstInspKind = mssql.Get<MST_INSP_KIND>();
                InspKindInfo.CodeList = mstInspKind.Select(x => x.INSP_KIND_CD).ToList();
                InspKindInfo.NameList = mstInspKind.Select(x => x.INSP_KIND_NM).ToList();
                InspKindInfo.Name = string.Empty;

                var nameList = new List<string>();
                foreach (var one in Enum.GetValues(typeof(ESimulResult)))
                {
                    nameList.Add(one.ToString());
                }
                SimulationResult.NameList = nameList;
                SimulationResult.Name = string.Empty;
            }
            catch (Exception ex)
            {
                AppLog.Error("InitializeCodeComboBox failed.", ex);
            }
        }

        private void InitializeCodeData()
        {
            try
            {
                var envVariable = LibEnvVariable.GetEnvVariable();

                SiteInfo.Code = envVariable.SITE_CD;
                FactoryInfo.Code = envVariable.FACT_CD;
                WarehouseInfo.Code = envVariable.WH_CD;
                ProcessInfo.Code = envVariable.PRCS_CD;
                MachineInfo.Code = envVariable.MC_CD;
                InspKindGroupInfo.Code = envVariable.INSP_KIND_GROUP_CD;
                InspKindInfo.Code = envVariable.INSP_KIND_CD;

                int index = SiteInfo.CodeList.IndexOf(SiteInfo.Code);
                if (index >= 0) SiteInfo.Name = SiteInfo.NameList[index]; else SiteInfo.Name = string.Empty;

                index = FactoryInfo.CodeList.IndexOf(FactoryInfo.Code);
                if (index >= 0) FactoryInfo.Name = FactoryInfo.NameList[index]; else FactoryInfo.Name = string.Empty;

                index = WarehouseInfo.CodeList.IndexOf(WarehouseInfo.Code);
                if (index >= 0) WarehouseInfo.Name = WarehouseInfo.NameList[index]; else WarehouseInfo.Name = string.Empty;

                index = ProcessInfo.CodeList.IndexOf(ProcessInfo.Code);
                if (index >= 0) ProcessInfo.Name = ProcessInfo.NameList[index]; else ProcessInfo.Name = string.Empty;

                index = MachineInfo.CodeList.IndexOf(MachineInfo.Code);
                if (index >= 0) MachineInfo.Name = MachineInfo.NameList[index]; else MachineInfo.Name = string.Empty;

                index = InspKindGroupInfo.CodeList.IndexOf(InspKindGroupInfo.Code);
                if (index >= 0) InspKindGroupInfo.Name = InspKindGroupInfo.NameList[index]; else InspKindGroupInfo.Name = string.Empty;

                index = InspKindInfo.CodeList.IndexOf(InspKindInfo.Code);
                if (index >= 0) InspKindInfo.Name = InspKindInfo.NameList[index]; else InspKindInfo.Name = string.Empty;

                SimulationResult.Name = envVariable.SIMUL_RESULT.ToString();
            }
            catch (Exception ex)
            {
                AppLog.Error("InitializeCodeData failed.", ex);
            }
        }

        public void CancelCodeSetting()
        {
            try
            {
                CodeErrorDescription = string.Empty;

                InitializeCodeData();
            }
            catch (Exception ex)
            {
                AppLog.Error("CancelCodeSetting failed.", ex);
            }
        }

        public void SaveCodeSetting()
        {
            try
            {
                CodeErrorDescription = string.Empty;

                bool result = SaveCodeData();
                if (!result) return;

                _allBatterySlotViewModel.Initialize();
            }
            catch (Exception ex)
            {
                AppLog.Error("SaveCodeSetting failed.", ex);
            }
        }

        private bool SaveCodeData()
        {
            try
            {
                if (!IsCodeDataInputValid())
                {
                    return false;
                }

                // 팝업
                bool result = new PopupInfoViewModel()
                {
                    Title = "코드 설정 값 저장",
                    Message = "설정 값을 저장하시겠습니까?",
                    CancelButtonVisibility = Visibility.Visible,
                }.Open();

                if (result)
                {
                    int index = SiteInfo.NameList.IndexOf(SiteInfo.Name);
                    if (index >= 0) SiteInfo.Code = SiteInfo.CodeList[index]; else SiteInfo.Code = string.Empty;
                    index = FactoryInfo.NameList.IndexOf(FactoryInfo.Name);
                    if (index >= 0) FactoryInfo.Code = FactoryInfo.CodeList[index]; else FactoryInfo.Code = string.Empty;
                    index = WarehouseInfo.NameList.IndexOf(WarehouseInfo.Name);
                    if (index >= 0) WarehouseInfo.Code = WarehouseInfo.CodeList[index]; else WarehouseInfo.Code = string.Empty;
                    index = ProcessInfo.NameList.IndexOf(ProcessInfo.Name);
                    if (index >= 0) ProcessInfo.Code = ProcessInfo.CodeList[index]; else ProcessInfo.Code = string.Empty;
                    index = MachineInfo.NameList.IndexOf(MachineInfo.Name);
                    if (index >= 0) MachineInfo.Code = MachineInfo.CodeList[index]; else MachineInfo.Code = string.Empty;
                    index = InspKindGroupInfo.NameList.IndexOf(InspKindGroupInfo.Name);
                    if (index >= 0) InspKindGroupInfo.Code = InspKindGroupInfo.CodeList[index]; else InspKindGroupInfo.Code = string.Empty;
                    index = InspKindInfo.NameList.IndexOf(InspKindInfo.Name);
                    if (index >= 0) InspKindInfo.Code = InspKindInfo.CodeList[index]; else InspKindInfo.Code = string.Empty;

                    EnvVariableData envVariable = new EnvVariableData
                    {
                        SITE_CD = SiteInfo.Code,
                        FACT_CD = FactoryInfo.Code,
                        WH_CD = WarehouseInfo.Code,
                        PRCS_CD = ProcessInfo.Code,
                        MC_CD = MachineInfo.Code,
                        INSP_KIND_GROUP_CD = InspKindGroupInfo.Code,
                        INSP_KIND_CD = InspKindInfo.Code,
                        SIMUL_RESULT = (ESimulResult)Enum.Parse(typeof(ESimulResult), SimulationResult.Name)
                    };
                    LibEnvVariable.UpdateCodeEnvVariable(envVariable);

                    AppLog.Trace($"Code settings saved: SITE_CD={envVariable.SITE_CD}, FACT_CD={envVariable.FACT_CD}, WH_CD={envVariable.WH_CD}, PRCS_CD={envVariable.PRCS_CD}, MC_CD={envVariable.MC_CD}, INSP_KIND_GROUP_CD={envVariable.INSP_KIND_GROUP_CD}, INSP_KIND_CD={envVariable.INSP_KIND_CD}, SIMUL_RESULT={envVariable.SIMUL_RESULT}");

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                AppLog.Error("SaveCodeData failed.", ex);
                return false;
            }
        }

        private bool IsCodeDataInputValid()
        {
            if (SiteInfo.Name == string.Empty)
            {
                CodeErrorDescription = "사이트를 선택해 주세요.";
                return false;
            }
            if (FactoryInfo.Name == string.Empty)
            {
                CodeErrorDescription = "공장을 선택해 주세요.";
                return false;
            }
            if (WarehouseInfo.Name == string.Empty)
            {
                CodeErrorDescription = "창고를 선택해 주세요.";
                return false;
            }
            if (ProcessInfo.Name == string.Empty)
            {
                CodeErrorDescription = "공정을 선택해 주세요.";
                return false;
            }
            if (MachineInfo.Name == string.Empty)
            {
                CodeErrorDescription = "설비를 선택해 주세요.";
                return false;
            }
            if (InspKindGroupInfo.Name == string.Empty)
            {
                CodeErrorDescription = "검사종류그룹을 입력해 주세요.";
                return false;
            }
            if (InspKindInfo.Name == string.Empty)
            {
                CodeErrorDescription = "검사종류를 입력해 주세요.";
                return false;
            }
            if (SimulationResult.Name == string.Empty)
            {
                CodeErrorDescription = "시뮬레이션 결과를 선택해 주세요.";
                return false;
            }

            CodeErrorDescription = "";

            return true;
        }
    }
}
