using MExpress.Mex;
using MsSqlProcessor.MintechSql;
using MsSqlProcessor.MsSql;
using OutcomeSystem.Design.BatteryTransferStatus;
using OutcomeSystem.Design.Common;
using OutcomeSystem.Design.Popup;
using OutcomeSystem.Library;
using OutcomeSystem.Repository;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace OutcomeSystem.Design.GradeClassBatterySearch
{
    public class GradeClassBatterySearchViewModel : BindableBase
    {
        private static GradeClassBatterySearchViewModel _instance;
        public static GradeClassBatterySearchViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GradeClassBatterySearchViewModel();
                }
                return _instance;
            }
        }

        public class GradeClassBatteryDB
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

        private BatteryTransferStatusViewModel _batteryTransferStatusViewModel = null;

        public SearchFilterBinding SearchFilter { get; set; } = new SearchFilterBinding();

        public ObservableCollection<BatteryInfoBinding> SearchedResult { get; set; } = new ObservableCollection<BatteryInfoBinding>();

        public void Loaded()
        {
            if (_batteryTransferStatusViewModel == null)
            {
                _batteryTransferStatusViewModel = BatteryTransferStatusViewModel.Instance;
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
            string GRADE_WH_CD = envVariables.GRADE_WH_CD;

            MLibMssql query = new MLibMssql();
            query.Query(nameof(INV_WAREHOUSE));
            query.Select<INV_WAREHOUSE>(
                nameof(INV_WAREHOUSE.ROW),
                nameof(INV_WAREHOUSE.COL),
                nameof(INV_WAREHOUSE.LVL),
                nameof(INV_WAREHOUSE.LBL_ID));
            query.Select<QLT_BTR_INSP>(
                nameof(QLT_BTR_INSP.INSP_GRD));
            query.Select<MST_SITE>(
                nameof(MST_SITE.SITE_NM));
            query.Select<MST_BTR>(
                nameof(MST_BTR.COLT_DAT),
                nameof(MST_BTR.COLT_RESN),
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
                nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.LBL_ID),
                nameof(QLT_BTR_INSP), nameof(QLT_BTR_INSP.LBL_ID));
            query.LeftJoin(
                nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.SITE_CD),
                nameof(MST_SITE), nameof(MST_SITE.SITE_CD));
            query.LeftJoin(
                nameof(QLT_BTR_INSP), nameof(QLT_BTR_INSP.LBL_ID),
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
            query.Where(nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.WH_CD), GRADE_WH_CD);
            query.Where(nameof(QLT_BTR_INSP), nameof(QLT_BTR_INSP.BTR_DIAG_STS), EIF_FLAG.Y);

            if (SearchFilter.LabelSubString != string.Empty)
            {
                query.Where(nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.LBL_ID), "LIKE", $"%{SearchFilter.LabelSubString}%");
            }

            if (SearchFilter.SelectedGrade != string.Empty)
            {
                query.Where(nameof(QLT_BTR_INSP), nameof(QLT_BTR_INSP.INSP_GRD), $"{SearchFilter.SelectedGrade}");
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

            var dbResult = await query.GetAsync<GradeClassBatteryDB>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                SearchedResult.Clear();
                foreach (var item in dbResult)
                {
                    var transferInfo = _batteryTransferStatusViewModel.RequestStatuses.ToList().Find(x => x.LabelId == item.LBL_ID);
                    if (transferInfo == null) // 전송 중이지 않은 배터리만 추가
                    {
                        BatteryInfoBinding binding = new BatteryInfoBinding()
                        {
                            Row = item.ROW,
                            Col = item.COL,
                            Lvl = item.LVL,
                            LabelId = item.LBL_ID,
                            Grade = item.INSP_GRD,
                            PackOrModule = item.PACK_MDLE_CD,
                            BatteryType = item.BTR_TYPE_NM,
                            CollectionDate = item.COLT_DAT,
                            CollectionReason = item.COLT_RESN,
                            ReleaseYear = item.CAR_RELS_YEAR,
                            CarManufacturer = item.CAR_MAKE_NM,
                            CarModel = item.CAR_NM,
                            BatteryManufacturer = item.BTR_MAKE_NM,
                            Site = item.SITE_NM
                        };
                        SearchedResult.Add(binding);
                    }
                }
            });
        }

        public void SelectOutcomeWaitSlot(OutcomeWaitSlotBinding outcomeWaitSlot)
        {
            var batteryInfo = outcomeWaitSlot.ParentBatteryInfo;

            // 기존에 선택된 슬롯을 동일하게 선택했을 경우 선택 해제
            if (object.ReferenceEquals(outcomeWaitSlot, batteryInfo.SelectedOutcomeWaitSlot))
            {
                // 기존에 선택된 슬롯 UI 업데이트
                {
                    outcomeWaitSlot.BorderBrush = ResColor.border_primary;
                    outcomeWaitSlot.Background = ResColor.surface_secondary;

                    // 다른 배터리 정보 UI 업데이트
                    foreach (var oneResult in SearchedResult)
                    {
                        if (!object.ReferenceEquals(oneResult, batteryInfo))
                        {
                            var unselectSlot = oneResult.OutcomeWaitSlots.ToList().Find(
                                x => x.Row == outcomeWaitSlot.Row &&
                                x.Col == outcomeWaitSlot.Col &&
                                x.Lvl == outcomeWaitSlot.Lvl);
                            unselectSlot.BorderBrush = ResColor.border_primary;
                            unselectSlot.Background = ResColor.surface_secondary;
                            unselectSlot.IsEnabled = true;
                            unselectSlot.Cursor = Cursors.Hand;
                        }
                    } 
                }

                batteryInfo.SelectedOutcomeWaitSlot = null;
            }
            else // 기존에 선택된 슬롯이 아닐 경우
            {
                // 기존에 선택된 슬롯 UI 업데이트
                if (batteryInfo.SelectedOutcomeWaitSlot != null)
                {
                    batteryInfo.SelectedOutcomeWaitSlot.BorderBrush = ResColor.border_primary;
                    batteryInfo.SelectedOutcomeWaitSlot.Background = ResColor.surface_secondary;

                    // 다른 배터리 정보 UI 업데이트
                    foreach (var oneResult in SearchedResult)
                    {
                        if (!object.ReferenceEquals(oneResult, batteryInfo))
                        {
                            var unselectSlot = oneResult.OutcomeWaitSlots.ToList().Find(
                                x => x.Row == batteryInfo.SelectedOutcomeWaitSlot.Row &&
                                x.Col == batteryInfo.SelectedOutcomeWaitSlot.Col &&
                                x.Lvl == batteryInfo.SelectedOutcomeWaitSlot.Lvl);
                            unselectSlot.BorderBrush = ResColor.border_primary;
                            unselectSlot.Background = ResColor.surface_secondary;
                            unselectSlot.IsEnabled = true;
                            unselectSlot.Cursor = Cursors.Hand;
                        }
                    }
                }

                // 새로 선택된 슬롯 UI 업데이트
                {
                    outcomeWaitSlot.BorderBrush = ResColor.border_secondary;
                    outcomeWaitSlot.Background = ResColor.surface_action_pressed2;

                    // 다른 배터리 정보 UI 업데이트
                    foreach (var oneResult in SearchedResult)
                    {
                        if (!object.ReferenceEquals(oneResult, batteryInfo))
                        {
                            var selectSlot = oneResult.OutcomeWaitSlots.ToList().Find(
                                x => x.Row == outcomeWaitSlot.Row &&
                                x.Col == outcomeWaitSlot.Col &&
                                x.Lvl == outcomeWaitSlot.Lvl);
                            selectSlot.BorderBrush = ResColor.border_disabled;
                            selectSlot.Background = ResColor.surface_disabled;
                            selectSlot.IsEnabled = false;
                            selectSlot.Cursor = Cursors.Arrow;
                        }
                    }
                }

                batteryInfo.SelectedOutcomeWaitSlot = outcomeWaitSlot;
            }
        }

        public void TransferBattery()
        {
            List<BatteryInfoBinding> searchedResultTemp = SearchedResult.ToList();

            foreach (var batteryInfo in searchedResultTemp)
            {
                // 선택된 슬롯이 없는 배터리는 건너뜀
                if (batteryInfo.SelectedOutcomeWaitSlot == null) continue;
                
                // 전송 배터리 리스트에 추가
                int row = batteryInfo.SelectedOutcomeWaitSlot.Row;
                int col = batteryInfo.SelectedOutcomeWaitSlot.Col;
                int lvl = batteryInfo.SelectedOutcomeWaitSlot.Lvl;

                var transferInfo = new TransferInfoBinding(row, lvl, col);
                transferInfo.LabelId = batteryInfo.LabelId;
                transferInfo.CarManufacturer = batteryInfo.CarManufacturer;
                transferInfo.CarModel = batteryInfo.CarModel;
                transferInfo.ReleaseYear = batteryInfo.ReleaseYear;
                transferInfo.Grade = batteryInfo.Grade;
                transferInfo.CollectionDate = batteryInfo.CollectionDate;
                transferInfo.CollectionReason = batteryInfo.CollectionReason;
                transferInfo.PackOrModule = batteryInfo.PackOrModule;
                transferInfo.BatteryManufacturer = batteryInfo.BatteryManufacturer;
                transferInfo.BatteryType = batteryInfo.BatteryType;
                transferInfo.Site = batteryInfo.Site;
                transferInfo.StatusVisibility = Visibility.Collapsed;
                transferInfo.CancelButtonVisibility = Visibility.Visible;
                transferInfo.StatusEnum = BatteryTransferStatusViewModel.ERequestStatus.Waiting;
                transferInfo.StatusText = "대기중";

                _batteryTransferStatusViewModel.RequestStatuses.Add(transferInfo);

                // 검색 결과에서 삭제
                SearchedResult.Remove(batteryInfo);
            }
        }

        public void ReturnBattery(TransferInfoBinding requestStatus)
        {
            // 검색 결과에 전송 배터리 정보 추가
            BatteryInfoBinding binding = new BatteryInfoBinding()
            {
                Row = requestStatus.Row,
                Col = requestStatus.Column,
                Lvl = requestStatus.Level,
                LabelId = requestStatus.LabelId,
                Grade = requestStatus.Grade,
                PackOrModule = requestStatus.PackOrModule,
                BatteryType = requestStatus.BatteryType,
                CollectionDate = requestStatus.CollectionDate,
                CollectionReason = requestStatus.CollectionReason,
                ReleaseYear = requestStatus.ReleaseYear,
                CarManufacturer = requestStatus.CarManufacturer,
                CarModel = requestStatus.CarModel,
                BatteryManufacturer = requestStatus.BatteryManufacturer,
                Site = requestStatus.Site
            };
            SearchedResult.Add(binding);

            // 출고 대기 장소 배터리 슬롯 활성화
            foreach (var batteryInfo in SearchedResult)
            {
                var unselectSlot = batteryInfo.OutcomeWaitSlots.ToList().Find(x => x.Row == requestStatus.Row && x.Col == requestStatus.Column && x.Lvl == requestStatus.Level);
                unselectSlot.BorderBrush = ResColor.border_primary;
                unselectSlot.Background = ResColor.surface_secondary;
                unselectSlot.IsEnabled = true;
                unselectSlot.Cursor = Cursors.Hand;
            }
        }
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

    public class BatteryInfoBinding : BindableBase
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

        private ObservableCollection<OutcomeWaitSlotBinding> _outcomeWaitSlots = new ObservableCollection<OutcomeWaitSlotBinding>();
        public ObservableCollection<OutcomeWaitSlotBinding> OutcomeWaitSlots
        {
            get => _outcomeWaitSlots;
            set => SetProperty(ref _outcomeWaitSlots, value);
        }

        private ObservableCollection<OutcomeWaitSlotBinding> _outcomeWaitSlots_1F = new ObservableCollection<OutcomeWaitSlotBinding>();
        public ObservableCollection<OutcomeWaitSlotBinding> OutcomeWaitSlots_1F
        {
            get => _outcomeWaitSlots_1F;
            set => SetProperty(ref _outcomeWaitSlots_1F, value);
        }

        private ObservableCollection<OutcomeWaitSlotBinding> _outcomeWaitSlots_2F = new ObservableCollection<OutcomeWaitSlotBinding>();
        public ObservableCollection<OutcomeWaitSlotBinding> OutcomeWaitSlots_2F
        {
            get => _outcomeWaitSlots_2F;
            set => SetProperty(ref _outcomeWaitSlots_2F, value);
        }

        private OutcomeWaitSlotBinding _selectedOutcomeWaitSlot = null;
        public OutcomeWaitSlotBinding SelectedOutcomeWaitSlot
        {
            get => _selectedOutcomeWaitSlot;
            set => SetProperty(ref _selectedOutcomeWaitSlot, value);
        }

        private AllBatterySlotViewModel _allBatterySlotViewModel = AllBatterySlotViewModel.Instance;
        private BatteryTransferStatusViewModel _batteryTransferStatusViewModel = BatteryTransferStatusViewModel.Instance;

        public BatteryInfoBinding()
        {
            InitializeOutcomeWaitSlots();
        }

        private void InitializeOutcomeWaitSlots()
        {
            OutcomeWaitSlots.Clear();
            OutcomeWaitSlots_1F.Clear();
            OutcomeWaitSlots_2F.Clear();

            var slots = _allBatterySlotViewModel.OutcomeWaitBatterySlots;

            for (int row = 1; row <= RepoConstant.OutcomeWaitingRowCount; row++)
            {
                for (int level = 1; level <= RepoConstant.OutcomeWaitingLevelCount; level++)
                {
                    for (int column = 1; column <= RepoConstant.OutcomeWaitingColumnCount; column++)
                    {
                        var slot = slots.ToList().Find(x => x.Row == row && x.Level == level && x.Column == column);
                        if (slot == null) continue;

                        var binding = new OutcomeWaitSlotBinding();
                        binding.Row = slot.Row;
                        binding.Col = slot.Column;
                        binding.Lvl = slot.Level;
                        binding.ParentBatteryInfo = this;

                        var transferInfo = _batteryTransferStatusViewModel.RequestStatuses.ToList().Find(x => x.Row == slot.Row && x.Level == slot.Level && x.Column == slot.Column);
                        if (transferInfo != null || slot.BatteryInfo.Label != string.Empty) // 전송 중이거나 출고대기 장소에 배터리가 있는 경우 
                        {
                            binding.Background = ResColor.surface_disabled;
                            binding.BorderBrush = ResColor.border_disabled;
                            binding.IsEnabled = false;
                            binding.Cursor = Cursors.Arrow;
                        }
                        else if (slot.BatteryInfo.Label == string.Empty) // 빈 슬롯인 경우
                        {
                            binding.Background = ResColor.surface_secondary;
                            binding.BorderBrush = ResColor.border_primary;
                            binding.IsEnabled = true;
                            binding.Cursor = Cursors.Hand;
                        }

                        OutcomeWaitSlots.Add(binding);
                        if (level == 1)
                        {
                            OutcomeWaitSlots_1F.Add(binding);
                        }
                        else if (level == 2)
                        {
                            OutcomeWaitSlots_2F.Add(binding);
                        }
                    }
                }
            }
        }
    }

    public class OutcomeWaitSlotBinding : BindableBase
    {
        private BatteryInfoBinding _parentBatteryInfo = null;
        public BatteryInfoBinding ParentBatteryInfo
        {
            get => _parentBatteryInfo;
            set => SetProperty(ref _parentBatteryInfo, value);
        }

        private int _row = 0;
        public int Row
        {
            get => _row;
            set => SetProperty(ref _row, value);
        }

        private int _col = 0;
        public int Col
        {
            get => _col;
            set => SetProperty(ref _col, value);
        }

        private int _lvl = 0;
        public int Lvl
        {
            get => _lvl;
            set => SetProperty(ref _lvl, value);
        }

        private SolidColorBrush _background = ResColor.surface_secondary;
        public SolidColorBrush Background
        {
            get => _background;
            set => SetProperty(ref _background, value);
        }

        private SolidColorBrush _borderBrush = ResColor.border_primary;
        public SolidColorBrush BorderBrush
        {
            get => _borderBrush;
            set => SetProperty(ref _borderBrush, value);
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private Cursor _cursor = Cursors.Hand;
        public Cursor Cursor
        {
            get => _cursor;
            set => SetProperty(ref _cursor, value);
        }
    }
}
