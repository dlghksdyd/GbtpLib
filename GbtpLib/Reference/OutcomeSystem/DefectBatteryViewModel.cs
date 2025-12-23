using MExpress.Mex;
using MsSqlProcessor.MintechSql;
using MsSqlProcessor.MsSql;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using OutcomeSystem.Design.Popup;
using OutcomeSystem.Library;
using OutcomeSystem.Design.Common;
using OutcomeSystem.Repository;
using OutcomeSystem.Design.OutcomeWaitBattery;
using System.Runtime.CompilerServices;

namespace OutcomeSystem.Design.DefectBattery
{
    public class DefectBatteryViewModel : BindableBase
    {
        private static DefectBatteryViewModel _instance = null;
        public static DefectBatteryViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DefectBatteryViewModel();
                }
                return _instance;
            }
        }

        public enum ERequestStatus
        {
            Waiting,
            InProgress,
            Finished,
        }

        public class DefectBatteryDB
        {
            // INV_WAREHOUSE
            public int ROW { get; set; }
            public int COL { get; set; }
            public int LVL { get; set; }
            public string LBL_ID { get; set; }
            // QLT_BTR_INSP
            public string INSP_GRD { get; set; }
            // MST_SITE
            public string SITE_NM { get; set; }
            // MST_BTR
            public string COLT_DAT { get; set; }
            public string COLT_RESN { get; set; }
            public string DIG_INSP_FLAG { get; set; }
            public string PACK_MDLE_CD { get; set; }
            // MST_BTR_TYPE
            public string BTR_TYPE_NM { get; set; }
            public string CAR_RELS_YEAR { get; set; }
            // MST_CAR_MAKE
            public string CAR_MAKE_NM { get; set; }
            // MST_CAR
            public string CAR_NM { get; set; }
            // MST_BTR_MAKE
            public string BTR_MAKE_NM { get; set; }
        }

        private Visibility _transferRequestButtonVisibility = Visibility.Visible;
        public Visibility TransferRequestButtonVisibility
        {
            get => _transferRequestButtonVisibility;
            set => SetProperty(ref _transferRequestButtonVisibility, value);
        }

        public SearchFilterBinding SearchFilter { get; set; } = new SearchFilterBinding();

        public ObservableCollection<SearchedBatteryBinding> SearchedResult { get; set; } = new ObservableCollection<SearchedBatteryBinding>();

        public TransferBatteryBinding TransferBattery { get; set; } = new TransferBatteryBinding();

        private AllBatterySlotViewModel _allBatterySlotViewModel = null;
        private OutcomeWaitBatteryViewModel _outcomeWaitBatteryViewModel = null;

        private SearchedBatteryBinding _selectedBattery = null;

        private Thread _transferThread = null;

        public void Loaded()
        {
            if (_allBatterySlotViewModel == null)
            {
                _allBatterySlotViewModel = AllBatterySlotViewModel.Instance;
            }

            if (_outcomeWaitBatteryViewModel == null)
            {
                _outcomeWaitBatteryViewModel = OutcomeWaitBatteryViewModel.Instance;
            }

            if (_transferThread == null)
            {
                _transferThread = new Thread(TransferThread);
                _transferThread.IsBackground = true;
                _transferThread.Start();
            }

            if (SearchFilter.CarManufactureList.Count == 0)
            {
                ThreadStart thread = new ThreadStart(InitializeFilter);

                new PopupWaitViewModel()
                {
                    Title = "필터 초기화 중",
                    CancelButtonVisibility = System.Windows.Visibility.Collapsed
                }.Open(thread);
            }
        }

        public void TransferThread()
        {
            while (true)
            {
                Thread.Sleep(1000);

                if (TransferBattery.Label == string.Empty) continue;

                if (TransferBattery.StatusEnum == ERequestStatus.Waiting)
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@IN_CMD_CD", EIF_CMD.EE7.ToString() },
                        { "@IN_DATA1", TransferBattery.Label },
                        { "@IN_DATA2", TransferBattery.DefectRow },
                        { "@IN_DATA3", TransferBattery.DefectCol },
                        { "@IN_DATA4", TransferBattery.DefectLvl },
                        { "@IN_DATA5", TransferBattery.LoadingRow },
                        { "@IN_DATA6", TransferBattery.LoadingColumn },
                        { "@IN_DATA7", TransferBattery.LoadingLevel },
                        { "@IN_REQ_SYS", RepoConstant.SystemCodeOutcome }
                    };
                    bool result = MssqlBasic.ExecuteStoredProcedure("BRDS_ITF_CMD_DATA_SET", parameters);
                    //bool result = true;
                    if (result)
                    {
                        TransferBattery.StatusEnum = ERequestStatus.InProgress;
                    }
                }
                else if (TransferBattery.StatusEnum == ERequestStatus.InProgress)
                {
                    var selectQueryBuilder = new MssqlSelectQueryBuilder();
                    selectQueryBuilder.AddSelectColumns<ITF_CMD_DATA>(new string[]
                    {
                        nameof(ITF_CMD_DATA.CMD_CD),
                    });
                    selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.DATA1), TransferBattery.Label);
                    selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.CMD_CD), EIF_CMD.EE8.ToString());
                    selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.IF_FLAG), EIF_FLAG.C.ToString());
                    var itfCmdDataList = MssqlBasic.Select<ITF_CMD_DATA>(nameof(ITF_CMD_DATA), selectQueryBuilder);
                    if (itfCmdDataList.Count >= 1)
                    {
                        var itfCmdData = itfCmdDataList.First();
                        var updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(ITF_CMD_DATA));
                        updateQueryBuilder.AddSet(nameof(ITF_CMD_DATA.IF_FLAG), EIF_FLAG.Y);
                        updateQueryBuilder.AddWhere(nameof(ITF_CMD_DATA.CMD_CD), EIF_CMD.EE8.ToString());
                        updateQueryBuilder.AddWhere(nameof(ITF_CMD_DATA.DATA1), TransferBattery.Label);
                        int result = MssqlBasic.Update<ITF_CMD_DATA>(updateQueryBuilder);
                        if (result >= 1)
                        {
                            TransferBattery.StatusEnum = ERequestStatus.Finished;
                        }
                    }
                }
                else if (TransferBattery.StatusEnum == ERequestStatus.Finished)
                {
                    // 상차장 DB 및 인스턴스 업데이트
                    _allBatterySlotViewModel.AddLoadingBattery(TransferBattery);

                    // 불량창고 DB 및 인스턴스 업데이트
                    DeleteDefectBattery(TransferBattery);
                }
            }
        }

        public void DeleteDefectBattery(TransferBatteryBinding requestStatus)
        {
            var envVariable = LibEnvVariable.GetEnvVariable();

            // DB 업데이트
            MssqlUpdateQueryBuilder updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(INV_WAREHOUSE));
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.LBL_ID), string.Empty);
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.LOAD_GRD), string.Empty);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.ROW), requestStatus.DefectRow);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.COL), requestStatus.DefectCol);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.LVL), requestStatus.DefectLvl);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.SITE_CD), envVariable.SITE_CD);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.FACT_CD), envVariable.FACT_CD);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.WH_CD), envVariable.DEFECT_WH_CD);

            MssqlBasic.Update<INV_WAREHOUSE>(updateQueryBuilder);

            // BatteryInfo 데이터 업데이트
            TransferBattery.Visibility = Visibility.Collapsed;
            TransferBattery.Label = string.Empty;
            TransferBattery.CarManufacturer = string.Empty;
            TransferBattery.CarModel = string.Empty;
            TransferBattery.ReleaseYear = string.Empty;
            TransferBattery.Grade = string.Empty;
            TransferBattery.CollectionDate = string.Empty;
            TransferBattery.CollectionReason = string.Empty;
            TransferBattery.PackOrModule = string.Empty;
            TransferBattery.BatteryManufacturer = string.Empty;
            TransferBattery.BatteryType = string.Empty;
            TransferBattery.Site = string.Empty;
            TransferBattery.DefectRow = 0;
            TransferBattery.DefectCol = 0;
            TransferBattery.DefectLvl = 0;
            TransferBattery.LoadingRow = 0;
            TransferBattery.LoadingColumn = 0;
            TransferBattery.LoadingLevel = 0;
            TransferBattery.StatusEnum = ERequestStatus.Waiting;

            // 버튼 visibility 업데이트
            TransferRequestButtonVisibility = Visibility.Visible;
        }

        public async void InitializeFilter()
        {
            MLibMssql mLibMssql = new MLibMssql();

            // 등급
            var mstCode = await mLibMssql.Query(nameof(MST_CODE)).
                Select(nameof(MST_CODE.COMM_CD_NM)).
                Where(nameof(MST_CODE), nameof(MST_CODE.COMM_CD_GROUP), RepoConstant.CommonCodeGroup_Grade).
                GetAsync<MST_CODE>();
            SearchFilter.GradeList = mstCode.ConvertAll(x => x.COMM_CD_NM).Distinct().ToList();

            // 차량 제조사
            var mstCarMake = await mLibMssql.Query(nameof(MST_CAR_MAKE)).Select(nameof(MST_CAR_MAKE.CAR_MAKE_NM)).GetAsync<MST_CAR_MAKE>();
            SearchFilter.CarManufactureList = mstCarMake.ConvertAll(x => x.CAR_MAKE_NM).Distinct().ToList();

            // 차종
            var mstCar = await mLibMssql.Query(nameof(MST_CAR)).Select(nameof(MST_CAR.CAR_NM)).GetAsync<MST_CAR>();
            SearchFilter.CarModelList = mstCar.ConvertAll(x => x.CAR_NM).Distinct().ToList();

            // 배터리 제조사
            var mstBtrMake = await mLibMssql.Query(nameof(MST_BTR_MAKE)).Select(nameof(MST_BTR_MAKE.BTR_MAKE_NM)).GetAsync<MST_BTR_MAKE>();
            SearchFilter.BatteryManufactureList = mstBtrMake.ConvertAll(x => x.BTR_MAKE_NM).Distinct().ToList();

            // 배터리 타입 & 출고년도
            var mstBtrType = await mLibMssql.Query(nameof(MST_BTR_TYPE)).Select(
                nameof(MST_BTR_TYPE.BTR_TYPE_NM), nameof(MST_BTR_TYPE.CAR_RELS_YEAR)).GetAsync<MST_BTR_TYPE>();
            SearchFilter.BatteryTypeList = mstBtrType.ConvertAll(x => x.BTR_TYPE_NM).Distinct().ToList();
            SearchFilter.ReleaseYearList = mstBtrType.ConvertAll(x => x.CAR_RELS_YEAR).Distinct().ToList();
        }

        public void ClearFilter()
        {
            SearchFilter.LabelSubString = string.Empty;
            SearchFilter.SelectedGrade = string.Empty;
            SearchFilter.StartCollectionDate = new DateTime(2000, 1, 1);
            SearchFilter.EndCollectionDate = new DateTime(2100, 12, 31);
            SearchFilter.CarManufacture = string.Empty;
            SearchFilter.CarModel = string.Empty;
            SearchFilter.BatteryManufacture = string.Empty;
            SearchFilter.ReleaseYear = string.Empty;
            SearchFilter.BatteryType = string.Empty;
        }

        public void Search()
        {
            ThreadStart thread = new ThreadStart(SearchThread);

            new PopupWaitViewModel()
            {
                Title = "검색 중",
                CancelButtonVisibility = System.Windows.Visibility.Collapsed
            }.Open(thread);
        }

        private async void SearchThread()
        {
            var envVariables = LibEnvVariable.GetEnvVariable();

            string SITE_CD = envVariables.SITE_CD;
            string FACT_CD = envVariables.FACT_CD;
            string DEFECT_WH_CD = envVariables.DEFECT_WH_CD;

            MLibMssql query = new MLibMssql();
            query.Query(nameof(INV_WAREHOUSE));
            query.Select<INV_WAREHOUSE>(
                nameof(INV_WAREHOUSE.ROW),
                nameof(INV_WAREHOUSE.COL),
                nameof(INV_WAREHOUSE.LVL),
                nameof(INV_WAREHOUSE.LBL_ID));
            query.Select<MST_SITE>(
                nameof(MST_SITE.SITE_NM));
            query.Select<MST_BTR>(
                nameof(MST_BTR.COLT_DAT),
                nameof(MST_BTR.COLT_RESN),
                nameof(MST_BTR.DIG_INSP_FLAG),
                nameof(MST_BTR.PACK_MDLE_CD));
            query.Select<MST_BTR_TYPE>(
                nameof(MST_BTR_TYPE.BTR_TYPE_NM),
                nameof(MST_BTR_TYPE.CAR_RELS_YEAR));
            query.Select<MST_CAR_MAKE>(
                nameof(MST_CAR_MAKE.CAR_MAKE_NM));
            query.Select<MST_CAR>(
                nameof(MST_CAR.CAR_NM));
            query.Select<MST_BTR_MAKE>(
                nameof(MST_BTR_MAKE.BTR_MAKE_NM));
            query.LeftJoin(
                nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.SITE_CD),
                nameof(MST_SITE), nameof(MST_SITE.SITE_CD));
            query.LeftJoin(
                nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.LBL_ID),
                nameof(MST_BTR), nameof(MST_BTR.LBL_ID));
            query.LeftJoin(
                nameof(MST_BTR), nameof(MST_BTR.BTR_TYPE_NO),
                nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.BTR_TYPE_NO));
            query.LeftJoin(
                nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.CAR_MAKE_CD),
                nameof(MST_CAR_MAKE), nameof(MST_CAR_MAKE.CAR_MAKE_CD));
            query.LeftJoin(
                nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.CAR_CD),
                nameof(MST_CAR), nameof(MST_CAR.CAR_CD));
            query.LeftJoin(
                nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.BTR_MAKE_CD),
                nameof(MST_BTR_MAKE), nameof(MST_BTR_MAKE.BTR_MAKE_CD));
            query.Where(nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.SITE_CD), SITE_CD);
            query.Where(nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.FACT_CD), FACT_CD);
            query.Where(nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.WH_CD), DEFECT_WH_CD);

            if (SearchFilter.LabelSubString != string.Empty)
            {
                query.Where(nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.LBL_ID), "LIKE", $"%{SearchFilter.LabelSubString}%");
            }

            if (SearchFilter.StartCollectionDate != null)
            {
                query.Where(nameof(MST_BTR), nameof(MST_BTR.COLT_DAT), ">=", SearchFilter.StartCollectionDate.ToString("yyyyMMdd"));
            }

            if (SearchFilter.EndCollectionDate != null)
            {
                query.Where(nameof(MST_BTR), nameof(MST_BTR.COLT_DAT), "<=", SearchFilter.EndCollectionDate.ToString("yyyyMMdd"));
            }

            if (SearchFilter.CarManufacture != string.Empty)
            {
                query.Where(nameof(MST_CAR_MAKE), nameof(MST_CAR_MAKE.CAR_MAKE_NM), SearchFilter.CarManufacture);
            }

            if (SearchFilter.CarModel != string.Empty)
            {
                query.Where(nameof(MST_CAR), nameof(MST_CAR.CAR_NM), SearchFilter.CarModel);
            }

            if (SearchFilter.BatteryManufacture != string.Empty)
            {
                query.Where(nameof(MST_BTR_MAKE), nameof(MST_BTR_MAKE.BTR_MAKE_NM), SearchFilter.BatteryManufacture);
            }

            if (SearchFilter.ReleaseYear != string.Empty)
            {
                query.Where(nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.CAR_RELS_YEAR), SearchFilter.ReleaseYear);
            }

            if (SearchFilter.BatteryType != string.Empty)
            {
                query.Where(nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.BTR_TYPE_NM), SearchFilter.BatteryType);
            }

            string selectQuery = query.GetSelectQuery();

            var dbResult = await query.GetAsync<DefectBatteryDB>();

            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                SearchedResult.Clear();
                foreach (var item in dbResult)
                {
                    // 전송 중이지 않은 배터리만 추가
                    if (this.TransferBattery.Label != item.LBL_ID)
                    {
                        SearchedBatteryBinding binding = new SearchedBatteryBinding()
                        {
                            Row = item.ROW,
                            Col = item.COL,
                            Lvl = item.LVL,
                            LabelId = item.LBL_ID,
                            PackOrModule = item.PACK_MDLE_CD,
                            BatteryType = item.BTR_TYPE_NM,
                            CollectionDate = item.COLT_DAT,
                            CollectionReason = item.COLT_RESN,
                            ReleaseYear = item.CAR_RELS_YEAR,
                            CarManufacturer = item.CAR_MAKE_NM,
                            CarModel = item.CAR_NM,
                            BatteryManufacturer = item.BTR_MAKE_NM,
                            Site = item.SITE_NM,
                            InspectionFlag = item.DIG_INSP_FLAG,
                        };
                        SearchedResult.Add(binding);
                    }
                }
            }));

            // 등급 할당 (진단 안했을 경우 F, 진단 했을 경우 진단 등급)
            foreach (var oneResult in SearchedResult)
            {
                if (oneResult.InspectionFlag == "Y")
                {
                    query = new MLibMssql();
                    query.Query(nameof(QLT_BTR_INSP));
                    query.Select(nameof(QLT_BTR_INSP.INSP_GRD));
                    query.Where(nameof(QLT_BTR_INSP), nameof(QLT_BTR_INSP.LBL_ID), oneResult.LabelId);
                    query.Where(nameof(QLT_BTR_INSP), nameof(QLT_BTR_INSP.INSP_KIND_CD), "DIAG");
                    query.OrderByDesc(nameof(QLT_BTR_INSP), nameof(QLT_BTR_INSP.INSP_SEQ));
                    var inspResult = await query.GetAsync<QLT_BTR_INSP>();

                    if (inspResult.Count >= 1)
                    {
                        oneResult.Grade = inspResult.First().INSP_GRD;
                    }
                    else
                    {
                        oneResult.Grade = "F";
                    }
                }
                else
                {
                    oneResult.Grade = "F";
                }
            }

            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                if (SearchFilter.SelectedGrade != string.Empty)
                {
                    var tempResult = SearchedResult.ToList();
                    SearchedResult.Clear();
                    SearchedResult.AddRange(tempResult.Where(x => x.Grade == SearchFilter.SelectedGrade));
                }
            }));
        }

        public void SelectBattery(SearchedBatteryBinding batteryToSelect)
        {
            // 테두리 UI 업데이트
            foreach (var battery in SearchedResult)
            {
                battery.BorderBrush = ResColor.border_primary;
                battery.BorderThicknessTop = new Thickness(1, 1, 1, 0);
                battery.BorderThicknessBottom = new Thickness(1, 0, 1, 1);
            }
            batteryToSelect.BorderBrush = ResColor.border_action;
            batteryToSelect.BorderThicknessTop = new Thickness(2, 2, 2, 0);
            batteryToSelect.BorderThicknessBottom = new Thickness(2, 0, 2, 2);

            _selectedBattery = batteryToSelect;
        }

        public void UnselectBattery()
        {
            // 테두리 UI 업데이트
            foreach (var battery in SearchedResult)
            {
                battery.BorderBrush = ResColor.border_primary;
                battery.BorderThicknessTop = new Thickness(1, 1, 1, 0);
                battery.BorderThicknessBottom = new Thickness(1, 0, 1, 1);
            }

            _selectedBattery = null;
        }

        public void AddTransferBattery()
        {
            // 배터리 선택 여부 확인
            if (_selectedBattery == null)
            {
                new PopupWarningViewModel()
                {
                    Title = "불량 출고 요청 실패",
                    Message = "선택된 배터리가 없습니다.",
                    CancelButtonVisibility = Visibility.Collapsed,
                }.Open();
                return;
            }

            // 출고 대기 장소에서 상차장으로 이송중인 배터리가 없는지 확인
            var reqStatus = _outcomeWaitBatteryViewModel.RequestStatuses.ToList().FindAll(x => x.AcceptVisibility == Visibility.Visible);
            if (reqStatus.Count >= 1)
            {
                new PopupWarningViewModel()
                {
                    Title = "불량 출고 요청 실패",
                    Message = "출고 대기 장소에서 상차장으로 이송중인 배터리가 있습니다.",
                    CancelButtonVisibility = Visibility.Collapsed,
                }.Open();
                return;
            }

            // 상차장에 배터리가 없는지 확인
            var loadingBatterySlots_1F = _allBatterySlotViewModel.LoadingBatterySlots_1F.First();
            if (loadingBatterySlots_1F.BatteryInfo.Label != string.Empty)
            {
                new PopupWarningViewModel()
                {
                    Title = "불량 출고 요청 실패",
                    Message = "상차장에 배터리가 있습니다. 상차장에 있는 배터리를 먼저 처리해 주세요.",
                    CancelButtonVisibility = Visibility.Collapsed,
                }.Open();
                return;
            }

            // 이송 배터리 설정
            TransferBattery.Visibility = Visibility.Visible;
            TransferBattery.Label = _selectedBattery.LabelId;
            TransferBattery.CarManufacturer = _selectedBattery.CarManufacturer;
            TransferBattery.CarModel = _selectedBattery.CarModel;
            TransferBattery.ReleaseYear = _selectedBattery.ReleaseYear;
            TransferBattery.Grade = _selectedBattery.Grade;
            TransferBattery.CollectionDate = _selectedBattery.CollectionDate;
            TransferBattery.CollectionReason = _selectedBattery.CollectionReason;
            TransferBattery.PackOrModule = _selectedBattery.PackOrModule;
            TransferBattery.BatteryManufacturer = _selectedBattery.BatteryManufacturer;
            TransferBattery.BatteryType = _selectedBattery.BatteryType;
            TransferBattery.Site = _selectedBattery.Site;
            TransferBattery.DefectRow = _selectedBattery.Row;
            TransferBattery.DefectCol = _selectedBattery.Col;
            TransferBattery.DefectLvl = _selectedBattery.Lvl;
            TransferBattery.LoadingRow = loadingBatterySlots_1F.Row;
            TransferBattery.LoadingColumn = loadingBatterySlots_1F.Column;
            TransferBattery.LoadingLevel = loadingBatterySlots_1F.Level;
            TransferBattery.StatusEnum = ERequestStatus.Waiting;

            // 검색 결과에서 삭제
            SearchedResult.Remove(_selectedBattery);

            // 배터리 선택 해제
            UnselectBattery();

            // 이송 요청 배터리 버튼 hidden
            TransferRequestButtonVisibility = Visibility.Hidden;
        }

        public class SearchFilterBinding : BindableBase
        {
            private string _labelSubString = string.Empty;
            public string LabelSubString
            {
                get => _labelSubString;
                set
                {
                    SetProperty(ref _labelSubString, value);
                }
            }

            private List<string> _gradeList = new List<string>();
            public List<string> GradeList
            {
                get => _gradeList;
                set
                {
                    SetProperty(ref _gradeList, value);
                }
            }

            private string _selectedGrade = string.Empty;
            public string SelectedGrade
            {
                get => _selectedGrade;
                set
                {
                    SetProperty(ref _selectedGrade, value);
                }
            }

            private DateTime _startCollectionDate = new DateTime(2000, 1, 1);
            public DateTime StartCollectionDate
            {
                get => _startCollectionDate;
                set => SetProperty(ref _startCollectionDate, value);
            }

            private DateTime _endCollectionDate = new DateTime(2100, 12, 31);
            public DateTime EndCollectionDate
            {
                get => _endCollectionDate;
                set => SetProperty(ref _endCollectionDate, value);
            }

            // 🔹 제조사 리스트
            private List<string> _carManufactureList = new List<string>();
            public List<string> CarManufactureList
            {
                get => _carManufactureList;
                set => SetProperty(ref _carManufactureList, value);
            }

            private string _carManufacture = string.Empty;
            public string CarManufacture
            {
                get => _carManufacture;
                set => SetProperty(ref _carManufacture, value);
            }

            // 🔹 차량 종류 리스트
            private List<string> _carModelList = new List<string>();
            public List<string> CarModelList
            {
                get => _carModelList;
                set => SetProperty(ref _carModelList, value);
            }

            private string _carModel = string.Empty;
            public string CarModel
            {
                get => _carModel;
                set => SetProperty(ref _carModel, value);
            }

            // 🔹 배터리 제조사 리스트
            private List<string> _batteryManufactureList = new List<string>();
            public List<string> BatteryManufactureList
            {
                get => _batteryManufactureList;
                set => SetProperty(ref _batteryManufactureList, value);
            }

            private string _batteryManufacture = string.Empty;
            public string BatteryManufacture
            {
                get => _batteryManufacture;
                set => SetProperty(ref _batteryManufacture, value);
            }

            // 🔹 출시년도 리스트
            private List<string> _releaseYearList = new List<string>();
            public List<string> ReleaseYearList
            {
                get => _releaseYearList;
                set => SetProperty(ref _releaseYearList, value);
            }

            private string _releaseYear = string.Empty;
            public string ReleaseYear
            {
                get => _releaseYear;
                set => SetProperty(ref _releaseYear, value);
            }

            // 🔹 배터리 유형 리스트
            private List<string> _batteryTypeList = new List<string>();
            public List<string> BatteryTypeList
            {
                get => _batteryTypeList;
                set => SetProperty(ref _batteryTypeList, value);
            }

            private string _batteryType = string.Empty;
            public string BatteryType
            {
                get => _batteryType;
                set => SetProperty(ref _batteryType, value);
            }
        }

        public class SearchedBatteryBinding : BindableBase
        {
            private int _row;
            public int Row
            {
                get => _row;
                set => SetProperty(ref _row, value);
            }

            private int _col;
            public int Col
            {
                get => _col;
                set => SetProperty(ref _col, value);
            }

            private int _lvl;
            public int Lvl
            {
                get => _lvl;
                set => SetProperty(ref _lvl, value);
            }

            private string _labelId = string.Empty;
            public string LabelId
            {
                get => _labelId;
                set => SetProperty(ref _labelId, value);
            }

            private string _carManufacturer = string.Empty;
            public string CarManufacturer
            {
                get => _carManufacturer;
                set => SetProperty(ref _carManufacturer, value);
            }

            private string _carModel = string.Empty;
            public string CarModel
            {
                get => _carModel;
                set => SetProperty(ref _carModel, value);
            }

            private string _releaseYear = string.Empty;
            public string ReleaseYear
            {
                get => _releaseYear;
                set => SetProperty(ref _releaseYear, value);
            }

            private string _batteryManufacturer = string.Empty;
            public string BatteryManufacturer
            {
                get => _batteryManufacturer;
                set => SetProperty(ref _batteryManufacturer, value);
            }

            private string _packOrModule = string.Empty;
            public string PackOrModule
            {
                get => _packOrModule;
                set => SetProperty(ref _packOrModule, value);
            }

            private string _batteryType = string.Empty;
            public string BatteryType
            {
                get => _batteryType;
                set => SetProperty(ref _batteryType, value);
            }

            private string _collectionDate = string.Empty;
            public string CollectionDate
            {
                get => _collectionDate;
                set => SetProperty(ref _collectionDate, value);
            }

            private string _collectionReason = string.Empty;
            public string CollectionReason
            {
                get => _collectionReason;
                set => SetProperty(ref _collectionReason, value);
            }

            private string _grade = string.Empty;
            public string Grade
            {
                get => _grade;
                set => SetProperty(ref _grade, value);
            }

            private string _site = string.Empty;
            public string Site
            {
                get => _site;
                set => SetProperty(ref _site, value);
            }

            private string _inspectionFlag = string.Empty; // Y or N
            public string InspectionFlag
            {
                get => _inspectionFlag;
                set => SetProperty(ref _inspectionFlag, value);
            }

            private SolidColorBrush _borderBrush = ResColor.border_primary;
            public SolidColorBrush BorderBrush
            {
                get => _borderBrush;
                set => SetProperty(ref _borderBrush, value);
            }

            private Thickness _borderThicknessTop = new Thickness(1, 1, 1, 0);
            public Thickness BorderThicknessTop
            {
                get => _borderThicknessTop;
                set => SetProperty(ref _borderThicknessTop, value);
            }

            private Thickness _borderThicknessBottom = new Thickness(1, 0, 1, 1);
            public Thickness BorderThicknessBottom
            {
                get => _borderThicknessBottom;
                set => SetProperty(ref _borderThicknessBottom, value);
            }
        }

        public class TransferBatteryBinding : BindableBase
        {
            private Visibility _visibility = Visibility.Collapsed;
            public Visibility Visibility
            {
                get => _visibility;
                set => SetProperty(ref _visibility, value);
            }

            private string _label = string.Empty;
            public string Label
            {
                get => _label;
                set => SetProperty(ref _label, value);
            }

            private string _carManufacturer = string.Empty;
            public string CarManufacturer
            {
                get => _carManufacturer;
                set => SetProperty(ref _carManufacturer, value);
            }

            private string _carModel = string.Empty;
            public string CarModel
            {
                get => _carModel;
                set => SetProperty(ref _carModel, value);
            }

            private string _batteryManufacturer = string.Empty;
            public string BatteryManufacturer
            {
                get => _batteryManufacturer;
                set => SetProperty(ref _batteryManufacturer, value);
            }

            private string _releaseYear = string.Empty;
            public string ReleaseYear
            {
                get => _releaseYear;
                set => SetProperty(ref _releaseYear, value);
            }

            private string _batteryType = string.Empty;
            public string BatteryType
            {
                get => _batteryType;
                set => SetProperty(ref _batteryType, value);
            }

            private string _packOrModule = string.Empty;
            public string PackOrModule
            {
                get => _packOrModule;
                set => SetProperty(ref _packOrModule, value);
            }

            private string _site = string.Empty;
            public string Site
            {
                get => _site;
                set => SetProperty(ref _site, value);
            }

            private string _collectionDate = string.Empty;
            public string CollectionDate
            {
                get => _collectionDate;
                set => SetProperty(ref _collectionDate, value);
            }

            private string _collectionReason = string.Empty;
            public string CollectionReason
            {
                get => _collectionReason;
                set => SetProperty(ref _collectionReason, value);
            }

            private string _grade = string.Empty;
            public string Grade
            {
                get => _grade;
                set => SetProperty(ref _grade, value);
            }

            private int _defectRow = 0;
            public int DefectRow
            {
                get => _defectRow;
                set => SetProperty(ref _defectRow, value);
            }

            private int _defectCol = 0;
            public int DefectCol
            {
                get => _defectCol;
                set => SetProperty(ref _defectCol, value);
            }

            private int _defectLvl = 0;
            public int DefectLvl
            {
                get => _defectLvl;
                set => SetProperty(ref _defectLvl, value);
            }

            private int _loadingRow = 0;
            public int LoadingRow
            {
                get => _loadingRow;
                set => SetProperty(ref _loadingRow, value);
            }

            private int _loadingColumn = 0;
            public int LoadingColumn
            {
                get => _loadingColumn;
                set => SetProperty(ref _loadingColumn, value);
            }

            private int _loadingLevel = 0;
            public int LoadingLevel
            {
                get => _loadingLevel;
                set => SetProperty(ref _loadingLevel, value);
            }

            private ERequestStatus _status = ERequestStatus.Waiting;
            public ERequestStatus StatusEnum
            {
                get => _status;
                set => SetProperty(ref _status, value);
            }
        }
    }
}
