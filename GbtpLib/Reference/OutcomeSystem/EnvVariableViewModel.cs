using OutcomeSystem.Design.Common;
using OutcomeSystem.Design.Popup;
using OutcomeSystem.Library;
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

namespace OutcomeSystem.Design.EnvVariable
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
        public PositionInfoBinding GradeClassWarehouseInfo { get; set; } = new PositionInfoBinding();
        public PositionInfoBinding OutcomeWaitWarehouseInfo { get; set; } = new PositionInfoBinding();
        public PositionInfoBinding DefectWarehouseInfo { get; set; } = new PositionInfoBinding();
        public PositionInfoBinding LoadingWarehouseInfo { get; set; } = new PositionInfoBinding();
        public IntSystemInfoBinding IntSystemInfo { get; set; } = new IntSystemInfoBinding();

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
            InitializeCodeComboBox();
        }

        public void Loaded()
        {
            _allBatterySlotViewModel = AllBatterySlotViewModel.Instance;

            InitializeExternalData();
            InitializeCodeData();
        }

        private void InitializeExternalData()
        {
            var envVariable = LibEnvVariable.GetEnvVariable();

            IntSystemInfo.IntSystemHost = envVariable.INT_SYSTEM_HOST;
            IntSystemInfo.IntSystemPort = envVariable.INT_SYSTEM_PORT;
        }

        public void CancelExternalSetting()
        {
            ExternalErrorDescription = string.Empty;

            InitializeExternalData();
        }

        public void SaveExternalSetting()
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
            });
            var popupWaitViewModel = new PopupWaitViewModel()
            {
                Title = "코드 정보 초기화 중",
            }.Open(thread);
        }

        private bool SaveExternalData()
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
                    INT_SYSTEM_HOST = IntSystemInfo.IntSystemHost,
                    INT_SYSTEM_PORT = IntSystemInfo.IntSystemPort,
                };
                LibEnvVariable.UpdateExternalEnvVariable(envVariable);

                return true;
            }

            return false;
        }

        private bool IsExternalDataInputValid()
        {
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
            GradeClassWarehouseInfo.CodeList = mstWarehouse.Select(x => x.WH_CD).ToList();
            GradeClassWarehouseInfo.NameList = mstWarehouse.Select(x => x.WH_NM).ToList();
            GradeClassWarehouseInfo.Name = string.Empty;

            mssql.Query(nameof(MST_WAREHOUSE));
            mssql.Select<MST_WAREHOUSE>(nameof(MST_WAREHOUSE.WH_CD));
            mssql.Select<MST_WAREHOUSE>(nameof(MST_WAREHOUSE.WH_NM));
            mssql.Where(nameof(MST_WAREHOUSE), nameof(MST_WAREHOUSE.USE_YN), EYN_FLAG.Y);
            mstWarehouse = mssql.Get<MST_WAREHOUSE>();
            OutcomeWaitWarehouseInfo.CodeList = mstWarehouse.Select(x => x.WH_CD).ToList();
            OutcomeWaitWarehouseInfo.NameList = mstWarehouse.Select(x => x.WH_NM).ToList();
            OutcomeWaitWarehouseInfo.Name = string.Empty;

            mssql.Query(nameof(MST_WAREHOUSE));
            mssql.Select<MST_WAREHOUSE>(nameof(MST_WAREHOUSE.WH_CD));
            mssql.Select<MST_WAREHOUSE>(nameof(MST_WAREHOUSE.WH_NM));
            mssql.Where(nameof(MST_WAREHOUSE), nameof(MST_WAREHOUSE.USE_YN), EYN_FLAG.Y);
            mstWarehouse = mssql.Get<MST_WAREHOUSE>();
            LoadingWarehouseInfo.CodeList = mstWarehouse.Select(x => x.WH_CD).ToList();
            LoadingWarehouseInfo.NameList = mstWarehouse.Select(x => x.WH_NM).ToList();
            LoadingWarehouseInfo.Name = string.Empty;

            mssql.Query(nameof(MST_WAREHOUSE));
            mssql.Select<MST_WAREHOUSE>(nameof(MST_WAREHOUSE.WH_CD));
            mssql.Select<MST_WAREHOUSE>(nameof(MST_WAREHOUSE.WH_NM));
            mssql.Where(nameof(MST_WAREHOUSE), nameof(MST_WAREHOUSE.USE_YN), EYN_FLAG.Y);
            mstWarehouse = mssql.Get<MST_WAREHOUSE>();
            DefectWarehouseInfo.CodeList = mstWarehouse.Select(x => x.WH_CD).ToList();
            DefectWarehouseInfo.NameList = mstWarehouse.Select(x => x.WH_NM).ToList();
            DefectWarehouseInfo.Name = string.Empty;
        }

        private void InitializeCodeData()
        {
            var envVariable = LibEnvVariable.GetEnvVariable();

            SiteInfo.Code = envVariable.SITE_CD;
            FactoryInfo.Code = envVariable.FACT_CD;
            GradeClassWarehouseInfo.Code = envVariable.GRADE_WH_CD;
            OutcomeWaitWarehouseInfo.Code = envVariable.OUTCOME_WH_CD;
            LoadingWarehouseInfo.Code = envVariable.LOADING_WH_CD;
            DefectWarehouseInfo.Code = envVariable.DEFECT_WH_CD;

            int index = SiteInfo.CodeList.IndexOf(SiteInfo.Code);
            if (index >= 0) SiteInfo.Name = SiteInfo.NameList[index]; else SiteInfo.Name = string.Empty;

            index = FactoryInfo.CodeList.IndexOf(FactoryInfo.Code);
            if (index >= 0) FactoryInfo.Name = FactoryInfo.NameList[index]; else FactoryInfo.Name = string.Empty;

            index = GradeClassWarehouseInfo.CodeList.IndexOf(GradeClassWarehouseInfo.Code);
            if (index >= 0) GradeClassWarehouseInfo.Name = GradeClassWarehouseInfo.NameList[index]; else GradeClassWarehouseInfo.Name = string.Empty;

            index = OutcomeWaitWarehouseInfo.CodeList.IndexOf(OutcomeWaitWarehouseInfo.Code);
            if (index >= 0) OutcomeWaitWarehouseInfo.Name = OutcomeWaitWarehouseInfo.NameList[index]; else OutcomeWaitWarehouseInfo.Name = string.Empty;

            index = LoadingWarehouseInfo.CodeList.IndexOf(LoadingWarehouseInfo.Code);
            if (index >= 0) LoadingWarehouseInfo.Name = LoadingWarehouseInfo.NameList[index]; else LoadingWarehouseInfo.Name = string.Empty;

            index = DefectWarehouseInfo.CodeList.IndexOf(DefectWarehouseInfo.Code);
            if (index >= 0) DefectWarehouseInfo.Name = DefectWarehouseInfo.NameList[index]; else DefectWarehouseInfo.Name = string.Empty;
        }

        public void CancelCodeSetting()
        {
            CodeErrorDescription = string.Empty;

            InitializeCodeData();
        }

        public void SaveCodeSetting()
        {
            CodeErrorDescription = string.Empty;

            bool result = SaveCodeData();
            if (!result) return;

            _allBatterySlotViewModel.Initialize();
        }

        private bool SaveCodeData()
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
                
                index = GradeClassWarehouseInfo.NameList.IndexOf(GradeClassWarehouseInfo.Name);
                if (index >= 0) GradeClassWarehouseInfo.Code = GradeClassWarehouseInfo.CodeList[index]; else GradeClassWarehouseInfo.Code = string.Empty;
                
                index = OutcomeWaitWarehouseInfo.NameList.IndexOf(OutcomeWaitWarehouseInfo.Name);
                if (index >= 0) OutcomeWaitWarehouseInfo.Code = OutcomeWaitWarehouseInfo.CodeList[index]; else OutcomeWaitWarehouseInfo.Code = string.Empty;
                
                index = LoadingWarehouseInfo.NameList.IndexOf(LoadingWarehouseInfo.Name);
                if (index >= 0) LoadingWarehouseInfo.Code = LoadingWarehouseInfo.CodeList[index]; else LoadingWarehouseInfo.Code = string.Empty;
                
                index = DefectWarehouseInfo.NameList.IndexOf(DefectWarehouseInfo.Name);
                if (index >= 0) DefectWarehouseInfo.Code = DefectWarehouseInfo.CodeList[index]; else DefectWarehouseInfo.Code = string.Empty;

                EnvVariableData envVariable = new EnvVariableData
                {
                    SITE_CD = SiteInfo.Code,
                    FACT_CD = FactoryInfo.Code,
                    GRADE_WH_CD = GradeClassWarehouseInfo.Code,
                    OUTCOME_WH_CD = OutcomeWaitWarehouseInfo.Code,
                    LOADING_WH_CD = LoadingWarehouseInfo.Code,
                    DEFECT_WH_CD = DefectWarehouseInfo.Code,
                };
                LibEnvVariable.UpdateCodeEnvVariable(envVariable);

                return true;
            }

            return false;
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
            if (GradeClassWarehouseInfo.Name == string.Empty)
            {
                CodeErrorDescription = "등급 분류 창고를 선택해 주세요.";
                return false;
            }
            if (OutcomeWaitWarehouseInfo.Name == string.Empty)
            {
                CodeErrorDescription = "출고 대기 창고를 선택해 주세요.";
                return false;
            }
            if (LoadingWarehouseInfo.Name == string.Empty)
            {
                CodeErrorDescription = "상차장을 선택해 주세요.";
                return false;
            }
            if (DefectWarehouseInfo.Name == string.Empty)
            {
                CodeErrorDescription = "결함 창고를 선택해 주세요.";
                return false;
            }

            CodeErrorDescription = "";

            return true;
        }
    }
}
