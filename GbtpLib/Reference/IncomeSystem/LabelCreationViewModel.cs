using IncomeSystem.Design.Common;
using IncomeSystem.Design.Popup;
using IncomeSystem.Library;
using IncomeSystem.Repository;
using MsSqlProcessor.MsSql;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IncomeSystem.Design.LabelManagement
{
    public class LabelCreationViewModel : BindableBase
    {
        public class LabelInfoBinding : BindableBase
        {
            private List<string> _carManufacturerNameList = new List<string>();
            public List<string> CarManufacturerNameList
            {
                get => _carManufacturerNameList;
                set => SetProperty(ref _carManufacturerNameList, value);
            }

            private List<string> _carManufacturerCodeList = new List<string>();
            public List<string> CarManufacturerCodeList
            {
                get => _carManufacturerCodeList;
                set => SetProperty(ref _carManufacturerCodeList, value);
            }

            private string _carManufacturer = string.Empty;
            public string CarManufacturer
            {
                get => _carManufacturer;
                set => SetProperty(ref _carManufacturer, value);
            }

            private List<string> _carModelNameList = new List<string>();
            public List<string> CarModelNameList
            {
                get => _carModelNameList;
                set => SetProperty(ref _carModelNameList, value);
            }

            private List<string> _carModelCodeList = new List<string>();
            public List<string> CarModelCodeList
            {
                get => _carModelCodeList;
                set => SetProperty(ref _carModelCodeList, value);
            }

            private string _carModel = string.Empty;
            public string CarModel
            {
                get => _carModel;
                set => SetProperty(ref _carModel, value);
            }

            private List<string> _batteryManufacturerNameList = new List<string>();
            public List<string> BatteryManufacturerNameList
            {
                get => _batteryManufacturerNameList;
                set => SetProperty(ref _batteryManufacturerNameList, value);
            }

            private List<string> _batteryManufacturerCodeList = new List<string>();
            public List<string> BatteryManufacturerCodeList
            {
                get => _batteryManufacturerCodeList;
                set => SetProperty(ref _batteryManufacturerCodeList, value);
            }

            private string _batteryManufacturer = string.Empty;
            public string BatteryManufacturer
            {
                get => _batteryManufacturer;
                set => SetProperty(ref _batteryManufacturer, value);
            }

            private List<string> _releaseYearNameList = new List<string>();
            public List<string> ReleaseYearNameList
            {
                get => _releaseYearNameList;
                set => SetProperty(ref _releaseYearNameList, value);
            }

            private List<string> _releaseYearCodeList = new List<string>();
            public List<string> ReleaseYearCodeList
            {
                get => _releaseYearCodeList;
                set => SetProperty(ref _releaseYearCodeList, value);
            }

            private string _releaseYear = string.Empty;
            public string ReleaseYear
            {
                get => _releaseYear;
                set => SetProperty(ref _releaseYear, value);
            }

            private List<string> _batteryTypeNameList = new List<string>();
            public List<string> BatteryTypeNameList
            {
                get => _batteryTypeNameList;
                set => SetProperty(ref _batteryTypeNameList, value);
            }

            private List<string> _batteryTypeCodeList = new List<string>();
            public List<string> BatteryTypeCodeList
            {
                get => _batteryTypeCodeList;
                set => SetProperty(ref _batteryTypeCodeList, value);
            }

            private List<int> _batteryTypeNumList = new List<int>();
            public List<int> BatteryTypeNumList
            {
                get => _batteryTypeNumList;
                set => SetProperty(ref _batteryTypeNumList, value);
            }

            private string _batteryTypeName = string.Empty;
            public string BatteryTypeName
            {
                get => _batteryTypeName;
                set => SetProperty(ref _batteryTypeName, value);
            }

            private int _batteryTypeNum = 0;
            public int BatteryTypeNum
            {
                get => _batteryTypeNum;
                set => SetProperty(ref _batteryTypeNum, value);
            }

            private List<string> _siteNameList = new List<string>();
            public List<string> SiteNameList
            {
                get => _siteNameList;
                set => SetProperty(ref _siteNameList, value);
            }

            private List<string> _siteCodeList = new List<string>();
            public List<string> SiteCodeList
            {
                get => _siteCodeList;
                set => SetProperty(ref _siteCodeList, value);
            }

            private string _site = string.Empty;
            public string Site
            {
                get => _site;
                set => SetProperty(ref _site, value);
            }

            private List<string> _collectionReasonNameList = new List<string>();
            public List<string> CollectionReasonNameList
            {
                get => _collectionReasonNameList;
                set => SetProperty(ref _collectionReasonNameList, value);
            }

            private List<string> _collectionReasonCodeList = new List<string>();
            public List<string> CollectionReasonCodeList
            {
                get => _collectionReasonCodeList;
                set => SetProperty(ref _collectionReasonCodeList, value);
            }

            private string _collectionReason = string.Empty;
            public string CollectionReason
            {
                get => _collectionReason;
                set => SetProperty(ref _collectionReason, value);
            }

            private string _collectionDate = string.Empty;
            public string CollectionDate
            {
                get => _collectionDate;
                set => SetProperty(ref _collectionDate, value);
            }

            private string _mile = string.Empty;
            public string Mile
            {
                get => _mile;
                set => SetProperty(ref _mile, value);
            }

            private string _memo = string.Empty;
            public string Memo
            {
                get => _memo;
                set => SetProperty(ref _memo, value);
            }
        }

        private static LabelCreationViewModel _instance;
        public static LabelCreationViewModel Instance
        {
            get => _instance ?? (_instance = new LabelCreationViewModel());
        }

        private LabelInfoBinding _labelInfo = new LabelInfoBinding();
        public LabelInfoBinding LabelInfo
        {
            get => _labelInfo;
            set => SetProperty(ref _labelInfo, value);
        }

        private LabelManagementViewModel _labelManagementViewModel = null;

        private BatterySlotViewModel _selectedBatteryInfo = null;

        public LabelCreationViewModel()
        {
            InitializeData();
        }

        public void InitializeData()
        {
            // 차량 제조사 콤보박스 초기화
            LabelInfo.CarManufacturerNameList = RepoInfos.LabelCreationInfos.Select(x => x.CAR_MAKE_NM).Distinct().ToList();
            LabelInfo.CarManufacturerCodeList = RepoInfos.LabelCreationInfos.Select(x => x.CAR_MAKE_CD).Distinct().ToList();
            LabelInfo.CarManufacturer = string.Empty;

            // 사업장 콤보박스 초기화
            var selectQueryBuilder = new MssqlSelectQueryBuilder();
            selectQueryBuilder.AddSelectColumns<MST_SITE>(
                new string[]
                {
                    nameof(MST_SITE.SITE_CD),
                    nameof(MST_SITE.SITE_NM),
                });
            selectQueryBuilder.AddWhere(nameof(MST_SITE), nameof(MST_SITE.USE_YN), EYN_FLAG.Y.ToString());
            var mstSiteList = MssqlBasic.Select<MST_SITE>(nameof(MST_SITE), selectQueryBuilder);
            LabelInfo.SiteNameList = mstSiteList.ConvertAll(x => x.SITE_NM).Distinct().ToList();
            LabelInfo.SiteCodeList = mstSiteList.ConvertAll(x => x.SITE_CD).Distinct().ToList();

            // 수거사유 콤보박스 초기화
            selectQueryBuilder = new MssqlSelectQueryBuilder();
            selectQueryBuilder.AddSelectColumns<MST_CODE>(
                new string[]
                {
                    nameof(MST_CODE.COMM_CD),
                    nameof(MST_CODE.COMM_CD_NM),
                });
            selectQueryBuilder.AddWhere(nameof(MST_CODE), nameof(MST_CODE.USE_YN), EYN_FLAG.Y.ToString());
            selectQueryBuilder.AddWhere(nameof(MST_CODE), nameof(MST_CODE.COMM_CD_GROUP), "COM016");
            var mstCodeList = MssqlBasic.Select<MST_CODE>(nameof(MST_CODE), selectQueryBuilder);
            LabelInfo.CollectionReasonNameList = mstCodeList.ConvertAll(x => x.COMM_CD_NM).Distinct().ToList();
            LabelInfo.CollectionReasonCodeList = mstCodeList.ConvertAll(x => x.COMM_CD).Distinct().ToList();
        }

        public void Update(BatterySlotViewModel batteryInfo)
        {
            _selectedBatteryInfo = batteryInfo;

            LabelInfo.CarManufacturer = string.Empty;
            LabelInfo.CarModel = string.Empty;
            LabelInfo.BatteryManufacturer = string.Empty;
            LabelInfo.ReleaseYear = string.Empty;
            LabelInfo.BatteryTypeName = string.Empty;
            LabelInfo.Site = string.Empty;
            LabelInfo.CollectionReason = string.Empty;
            LabelInfo.CollectionDate = DateTime.Now.ToString("yyyy-MM-dd");
            LabelInfo.Mile = string.Empty;
            LabelInfo.Memo = string.Empty;
        }

        public void SelectCarManufacturerCombobox()
        {
            var searchedInfos = RepoInfos.LabelCreationInfos.FindAll(x => x.CAR_MAKE_NM == _labelInfo.CarManufacturer);
            LabelInfo.CarModelNameList = searchedInfos.ConvertAll(x => x.CAR_NM).Distinct().ToList();
            LabelInfo.CarModelCodeList = searchedInfos.ConvertAll(x => x.CAR_CD).Distinct().ToList();
            LabelInfo.CarModel = string.Empty;

            LabelInfo.BatteryManufacturerNameList = new List<string>();
            LabelInfo.BatteryManufacturer = string.Empty;

            LabelInfo.ReleaseYearNameList = new List<string>();
            LabelInfo.ReleaseYear = string.Empty;

            LabelInfo.BatteryTypeNameList = new List<string>();
            LabelInfo.BatteryTypeName = string.Empty;
        }

        public void SelectCarModelCombobox()
        {
            var searchedInfos = RepoInfos.LabelCreationInfos.FindAll(
                x => x.CAR_MAKE_NM == _labelInfo.CarManufacturer &&
                x.CAR_NM == _labelInfo.CarModel);
            LabelInfo.BatteryManufacturerNameList = searchedInfos.ConvertAll(x => x.BTR_MAKE_NM).Distinct().ToList();
            LabelInfo.BatteryManufacturerCodeList = searchedInfos.ConvertAll(x => x.BTR_MAKE_CD).Distinct().ToList();
            LabelInfo.BatteryManufacturer = string.Empty;

            LabelInfo.ReleaseYearNameList = new List<string>();
            LabelInfo.ReleaseYear = string.Empty;

            LabelInfo.BatteryTypeNameList = new List<string>();
            LabelInfo.BatteryTypeName = string.Empty;
        }

        public void SelectBatteryManufacturerCombobox()
        {
            var searchedInfos = RepoInfos.LabelCreationInfos.FindAll(
                x => x.CAR_MAKE_NM == _labelInfo.CarManufacturer &&
                x.CAR_NM == _labelInfo.CarModel &&
                x.BTR_MAKE_NM == _labelInfo.BatteryManufacturer);
            LabelInfo.ReleaseYearNameList = searchedInfos.ConvertAll(x => x.CAR_RELS_YEAR).Distinct().ToList();
            LabelInfo.ReleaseYearCodeList = searchedInfos.ConvertAll(x => x.CAR_RELS_YEAR.Substring(2)).Distinct().ToList();
            LabelInfo.ReleaseYear = string.Empty;

            LabelInfo.BatteryTypeNameList = new List<string>();
            LabelInfo.BatteryTypeName = string.Empty;
        }

        public void SelectReleaseYearCombobox()
        {
            var searchedInfos = RepoInfos.LabelCreationInfos.FindAll(
                x => x.CAR_MAKE_NM == _labelInfo.CarManufacturer &&
                x.CAR_NM == _labelInfo.CarModel &&
                x.BTR_MAKE_NM == _labelInfo.BatteryManufacturer &&
                x.CAR_RELS_YEAR == _labelInfo.ReleaseYear);
            LabelInfo.BatteryTypeNameList = searchedInfos.ConvertAll(x => x.BTR_TYPE_NM).Distinct().ToList();
            LabelInfo.BatteryTypeCodeList = searchedInfos.ConvertAll(x => x.BTR_TYPE_SLT_CD).Distinct().ToList();
            LabelInfo.BatteryTypeNumList = searchedInfos.ConvertAll(x => x.BTR_TYPE_NO).Distinct().ToList();
            LabelInfo.BatteryTypeName = string.Empty;
            LabelInfo.BatteryTypeNum = 0;
        }

        public void SelectBatteryTypeCombobox()
        {
            int index = LabelInfo.BatteryTypeNameList.FindIndex(x => x == LabelInfo.BatteryTypeName);
            if (index >= 0)
            {
                LabelInfo.BatteryTypeNum = LabelInfo.BatteryTypeNumList[index];
            }
        }

        public void CreateLabel()
        {
            if (_labelInfo.CarManufacturer == string.Empty)
            {
                new PopupWarningViewModel()
                {
                    Title = "오류",
                    Message = "차량 제조사를 선택하지 않았습니다.",
                    CancelButtonVisibility = Visibility.Collapsed,
                }.Open();

                return;
            }

            if (_labelInfo.CarModel == string.Empty)
            {
                new PopupWarningViewModel()
                {
                    Title = "오류",
                    Message = "차종을 선택하지 않았습니다.",
                    CancelButtonVisibility = Visibility.Collapsed,
                }.Open();

                return;
            }

            if (_labelInfo.BatteryManufacturer == string.Empty)
            {
                new PopupWarningViewModel()
                {
                    Title = "오류",
                    Message = "배터리 제조사를 선택하지 않았습니다.",
                    CancelButtonVisibility = Visibility.Collapsed,
                }.Open();

                return;
            }

            if (_labelInfo.ReleaseYear == string.Empty)
            {
                new PopupWarningViewModel()
                {
                    Title = "오류",
                    Message = "출고년도를 선택하지 않았습니다.",
                    CancelButtonVisibility = Visibility.Collapsed,
                }.Open();

                return;
            }

            if (_labelInfo.BatteryTypeName == string.Empty)
            {
                new PopupWarningViewModel()
                {
                    Title = "오류",
                    Message = "배터리 타입을 선택하지 않았습니다.",
                    CancelButtonVisibility = Visibility.Collapsed,
                }.Open();

                return;
            }

            if (_labelInfo.Site == string.Empty)
            {
                new PopupWarningViewModel()
                {
                    Title = "오류",
                    Message = "사업장을 선택하지 않았습니다.",
                    CancelButtonVisibility = Visibility.Collapsed,
                }.Open();

                return;
            }

            if (_labelInfo.CollectionReason == string.Empty)
            {
                new PopupWarningViewModel()
                {
                    Title = "오류",
                    Message = "수거사유를 선택하지 않았습니다.",
                    CancelButtonVisibility = Visibility.Collapsed,
                }.Open();

                return;
            }

            if (_labelInfo.CollectionDate == string.Empty)
            {
                new PopupWarningViewModel()
                {
                    Title = "오류",
                    Message = "수거일자를 선택하지 않았습니다.",
                    CancelButtonVisibility = Visibility.Collapsed,
                }.Open();

                return;
            }

            // DB 업데이트
            bool result = InsertLabelInfo(out string labelId);
            if (!result)
            {
                new PopupWarningViewModel()
                {
                    Title = "라벨 생성",
                    Message = "라벨 생성에 실패하였습니다.",
                    CancelButtonVisibility = Visibility.Collapsed,
                }.Open();

                return;
            }
            result = UpdateWarehouseInfo(labelId);
            if (!result)
            {
                // 라벨 생성 롤백
                DeleteLabelInfo(labelId);

                new PopupWarningViewModel()
                {
                    Title = "라벨 생성",
                    Message = "라벨 생성에 실패하였습니다.",
                    CancelButtonVisibility = Visibility.Collapsed,
                }.Open();

                return;
            }

            // 인스턴스 업데이트
            UpdateSelectedBatteryInfo(labelId);
        }

        private MST_BTR_TYPE GetMstBtrType(int batteryTypeNo)
        {
            // 배터리 타입 정보 추가
            var selectQueryBuilder = new MssqlSelectQueryBuilder();
            selectQueryBuilder.AddSelectColumns<MST_BTR_TYPE>(
                new string[]
                {
                    nameof(MST_BTR_TYPE.PACK_MDLE_CD),
                    nameof(MST_BTR_TYPE.VOLT_MAX_VALUE),
                    nameof(MST_BTR_TYPE.VOLT_MIN_VALUE),
                    nameof(MST_BTR_TYPE.ACIR_MAX_VALUE),
                    nameof(MST_BTR_TYPE.INSUL_MIN_VALUE),
                });
            selectQueryBuilder.AddWhere(nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.BTR_TYPE_NO), batteryTypeNo);
            var mstBtrTypeList = MssqlBasic.Select<MST_BTR_TYPE>(nameof(MST_BTR_TYPE), selectQueryBuilder);
            if (mstBtrTypeList.Count >= 1)
            {
                var mstBtrType = mstBtrTypeList.First();
                _selectedBatteryInfo.BatteryInfo.PackOrModule = mstBtrType.PACK_MDLE_CD;
                _selectedBatteryInfo.BatteryInfo.VoltMaxValue = mstBtrType.VOLT_MAX_VALUE;
                _selectedBatteryInfo.BatteryInfo.VoltMinValue = mstBtrType.VOLT_MIN_VALUE;
                _selectedBatteryInfo.BatteryInfo.AcirMaxValue = mstBtrType.ACIR_MAX_VALUE;
                _selectedBatteryInfo.BatteryInfo.InsulationMinValue = mstBtrType.INSUL_MIN_VALUE;

                return mstBtrType;
            }
            else
            {
                return null;
            }
        }

        private void UpdateSelectedBatteryInfo(string labelId)
        {
            _labelManagementViewModel = LabelManagementViewModel.Instance;

            _selectedBatteryInfo.BatteryInfo.Label = labelId;
            _selectedBatteryInfo.BatteryInfo.CarManufacturer = LabelInfo.CarManufacturer;
            _selectedBatteryInfo.BatteryInfo.CarModel = LabelInfo.CarModel;
            _selectedBatteryInfo.BatteryInfo.BatteryManufacturer = LabelInfo.BatteryManufacturer;
            _selectedBatteryInfo.BatteryInfo.ReleaseYear = LabelInfo.ReleaseYear;
            _selectedBatteryInfo.BatteryInfo.BatteryType = LabelInfo.BatteryTypeName;
            _selectedBatteryInfo.BatteryInfo.Site = LabelInfo.Site;
            _selectedBatteryInfo.BatteryInfo.CollectionReason = LabelInfo.CollectionReason;
            _selectedBatteryInfo.BatteryInfo.CollectionDate = LabelInfo.CollectionDate;
            _selectedBatteryInfo.BatteryInfo.Mile = (LabelInfo.Mile != string.Empty) ? int.Parse(LabelInfo.Mile): 0;
            _selectedBatteryInfo.BatteryInfo.Note = LabelInfo.Memo;

            _selectedBatteryInfo.BatteryInfo.EmptyVisibility = Visibility.Collapsed;
            _selectedBatteryInfo.BatteryInfo.LabelDontExistVisibility = Visibility.Collapsed;

            // 배터리 타입 정보 추가
            var mstBtrType = GetMstBtrType(LabelInfo.BatteryTypeNum);
            if (mstBtrType != null)
            {
                _selectedBatteryInfo.BatteryInfo.PackOrModule = mstBtrType.PACK_MDLE_CD;
                _selectedBatteryInfo.BatteryInfo.VoltMaxValue = mstBtrType.VOLT_MAX_VALUE;
                _selectedBatteryInfo.BatteryInfo.VoltMinValue = mstBtrType.VOLT_MIN_VALUE;
                _selectedBatteryInfo.BatteryInfo.AcirMaxValue = mstBtrType.ACIR_MAX_VALUE;
                _selectedBatteryInfo.BatteryInfo.InsulationMinValue = mstBtrType.INSUL_MIN_VALUE;
            }

            _labelManagementViewModel.ClearSlotSelection();
        }

        private bool InsertLabelInfo(out string labelId)
        {
            string collectionDay = LabelInfo.CollectionDate.Replace("-", "");

            var selectQueryBuilder = new MssqlSelectQueryBuilder();
            selectQueryBuilder.AddSelectColumns<MST_BTR>(
                new string[]
                {
                    nameof(MST_BTR.VER),
                });
            selectQueryBuilder.AddWhere(nameof(MST_BTR), nameof(MST_BTR.COLT_DAT), collectionDay);
            var mstBtrList = MssqlBasic.Select<MST_BTR>(nameof(MST_BTR), selectQueryBuilder);

            int serialNum = 0;
            foreach (var oneBtr in mstBtrList)
            {
                if (oneBtr.VER > serialNum)
                {
                    serialNum = oneBtr.VER;
                }
            }
            serialNum += 1; /// 시리얼 넘버 1증가

            // 코드 정보 가져오기
            int selectedIndex = _labelInfo.CarManufacturerNameList.FindIndex(x => x == _labelInfo.CarManufacturer);
            string carManufacturerCode = _labelInfo.CarManufacturerCodeList[selectedIndex];

            selectedIndex = _labelInfo.CarModelNameList.FindIndex(x => x == _labelInfo.CarModel);
            string carModelCode = _labelInfo.CarModelCodeList[selectedIndex];

            selectedIndex = _labelInfo.BatteryManufacturerNameList.FindIndex(x => x == _labelInfo.BatteryManufacturer);
            string batteryManufacturerCode = _labelInfo.BatteryManufacturerCodeList[selectedIndex];

            selectedIndex = _labelInfo.ReleaseYearNameList.FindIndex(x => x == _labelInfo.ReleaseYear);
            string releaseYearCode = _labelInfo.ReleaseYearCodeList[selectedIndex];

            selectedIndex = _labelInfo.BatteryTypeNameList.FindIndex(x => x == _labelInfo.BatteryTypeName);
            string batteryTypeCode = _labelInfo.BatteryTypeCodeList[selectedIndex];
            int batteryTypeNum = _labelInfo.BatteryTypeNumList[selectedIndex];
            string packOrModule = string.Empty;
            var mstBtrType = GetMstBtrType(LabelInfo.BatteryTypeNum);
            if (mstBtrType != null) packOrModule = mstBtrType.PACK_MDLE_CD;

            selectedIndex = _labelInfo.SiteNameList.FindIndex(x => x == _labelInfo.Site);
            string siteCode = _labelInfo.SiteCodeList[selectedIndex];

            selectedIndex = _labelInfo.CollectionReasonNameList.FindIndex(x => x == _labelInfo.CollectionReason);
            string collectionReasonCode = _labelInfo.CollectionReasonCodeList[selectedIndex];
            string collectionReasonName = _labelInfo.CollectionReasonNameList[selectedIndex];

            // 라벨 ID 생성
            labelId = string.Empty;
            labelId += carManufacturerCode;
            labelId += carModelCode;
            labelId += releaseYearCode;
            labelId += packOrModule;
            labelId += batteryManufacturerCode;
            labelId += batteryTypeCode;
            labelId += "_";
            labelId += siteCode;
            labelId += "_";
            labelId += collectionReasonCode;
            labelId += collectionDay;
            labelId += serialNum.ToString("D3");

            // 배터리 정보 저장
            var insertQueryBuilder = new MssqlInsertQueryBuilder(nameof(MST_BTR));
            insertQueryBuilder.AddColumn(nameof(MST_BTR.LBL_ID), labelId);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.BTR_TYPE_NO), batteryTypeNum);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.PACK_MDLE_CD), "P");
            insertQueryBuilder.AddColumn(nameof(MST_BTR.SITE_CD), siteCode);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.COLT_DAT), collectionDay);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.COLT_RESN), collectionReasonName);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.MUFT_DAT), string.Empty);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.MILE), (_labelInfo.Mile != string.Empty ? int.Parse(_labelInfo.Mile) : 0));
            insertQueryBuilder.AddColumn(nameof(MST_BTR.VER), serialNum);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.NOTE), _labelInfo.Memo);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.PRT_YN), EYN_FLAG.N);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.BTR_STS), MssqlProcedure.GetCode_BatteryState("입고검사 대기"));
            insertQueryBuilder.AddColumn(nameof(MST_BTR.STO_INSP_FLAG), EYN_FLAG.N);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.ENE_INSP_FLAG), EYN_FLAG.N);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.ENE_INSP_CNT), 0);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.DIG_INSP_FLAG), EYN_FLAG.N);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.EMG_OUT_FLAG), EYN_FLAG.N);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.EMG_OUT_NOTE), string.Empty);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.USE_YN), EYN_FLAG.Y);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.REG_ID), RepoUser.UserId);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.REG_DTM), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            insertQueryBuilder.AddColumn(nameof(MST_BTR.MOD_ID), string.Empty);
            insertQueryBuilder.AddColumn(nameof(MST_BTR.MOD_DTM), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            int result = MssqlBasic.Insert<MST_BTR>(insertQueryBuilder);

            if (result >= 0) return true; else return false;
        }

        private bool DeleteLabelInfo(string labelId)
        {
            var deleteQueryBuilder = new MssqlDeleteQueryBuilder(nameof(MST_BTR));
            deleteQueryBuilder.AddWhere(nameof(MST_BTR.LBL_ID), labelId);
            int result = MssqlBasic.Delete<MST_BTR>(deleteQueryBuilder);
            if (result >= 0) return true; else return false;
        }

        private bool UpdateWarehouseInfo(string labelId)
        {
            EnvVariableData envData = LibEnvVariable.GetEnvVariable();

            var updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(INV_WAREHOUSE));
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.LBL_ID), labelId);
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.LOAD_GRD), string.Empty);
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.STORE_DIV), "02");
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.STS), string.Empty);
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.NOTE), string.Empty);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.SITE_CD), envData.SITE_CD);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.FACT_CD), envData.FACT_CD);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.WH_CD), envData.WH_CD);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.ROW), _selectedBatteryInfo.Row.ToString());
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.COL), _selectedBatteryInfo.Column.ToString());
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.LVL), _selectedBatteryInfo.Level.ToString());
            int result = MssqlBasic.Update<INV_WAREHOUSE>(updateQueryBuilder);

            if (result >= 0) return true; else return false;
        }
    }
}
