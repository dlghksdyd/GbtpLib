using MExpress.Mex;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using OutcomeSystem.Design.Popup;
using OutcomeSystem.Repository;
using MsSqlProcessor.MsSql;
using System.Collections.ObjectModel;
using System.Threading;
using OutcomeSystem.Library;
using MsSqlProcessor.MintechSql;
using OutcomeSystem.Design.BatteryTransferStatus;
using OutcomeSystem.Design.OutcomeWaitBattery;
using OutcomeSystem.Design.DefectBattery;

namespace OutcomeSystem.Design.Common
{
    public class AllBatterySlotViewModel : BindableBase
    {
        private static AllBatterySlotViewModel _instance = null;
        public static AllBatterySlotViewModel Instance
        {
            get => _instance ?? (_instance = new AllBatterySlotViewModel());
        }

        public class SlotInfo
        {
            // INV_WAREHOUSE
            public int ROW { get; set; } = 0;
            public int COL { get; set; } = 0;
            public int LVL { get; set; } = 0;
            public string LBL_ID { get; set; } = string.Empty;
            // QLT_BTR_INSP
            public string INSP_GRD { get; set; }
            // MST_SITE
            public string SITE_NM { get; set; } = string.Empty;
            // MST_BTR
            public string COLT_DAT { get; set; } = string.Empty;
            public string COLT_RESN { get; set; } = string.Empty;
            public string PACK_MDLE_CD { get; set; } = string.Empty;
            // MST_BTR_TYPE
            public string BTR_TYPE_NM { get; set; } = string.Empty;
            public string CAR_RELS_YEAR { get; set; } = string.Empty;
            // MST_CAR_MAKE
            public string CAR_MAKE_NM { get; set; } = string.Empty;
            // MST_CAR
            public string CAR_NM { get; set; } = string.Empty;
            // MST_BTR_MAKE
            public string BTR_MAKE_NM { get; set; } = string.Empty;
        }

        private ObservableCollection<OutcomeWaitBatterySlotViewModel> _outcomeWaitbatterySlots = new ObservableCollection<OutcomeWaitBatterySlotViewModel>();
        public ObservableCollection<OutcomeWaitBatterySlotViewModel> OutcomeWaitBatterySlots
        {
            get => _outcomeWaitbatterySlots;
            set => SetProperty(ref _outcomeWaitbatterySlots, value);
        }

        private ObservableCollection<OutcomeWaitBatterySlotViewModel> _outcomeWaitBatterySlots_2F = new ObservableCollection<OutcomeWaitBatterySlotViewModel>();
        public ObservableCollection<OutcomeWaitBatterySlotViewModel> OutcomeWaitBatterySlots_2F
        {
            get => _outcomeWaitBatterySlots_2F;
            set => SetProperty(ref _outcomeWaitBatterySlots_2F, value);
        }

        private ObservableCollection<OutcomeWaitBatterySlotViewModel> _outcomeWaitBatterySlots_1F = new ObservableCollection<OutcomeWaitBatterySlotViewModel>();
        public ObservableCollection<OutcomeWaitBatterySlotViewModel> OutcomeWaitBatterySlots_1F
        {
            get => _outcomeWaitBatterySlots_1F;
            set => SetProperty(ref _outcomeWaitBatterySlots_1F, value);
        }

        private ObservableCollection<LoadingBatterySlotViewModel> _loadingBatterySlots = new ObservableCollection<LoadingBatterySlotViewModel>();
        public ObservableCollection<LoadingBatterySlotViewModel> LoadingBatterySlots
        {
            get => _loadingBatterySlots;
            set => SetProperty(ref _loadingBatterySlots, value);
        }

        private ObservableCollection<LoadingBatterySlotViewModel> _loadingBatterySlots_3F = new ObservableCollection<LoadingBatterySlotViewModel>();
        public ObservableCollection<LoadingBatterySlotViewModel> LoadingBatterySlots_3F
        {
            get => _loadingBatterySlots_3F;
            set => SetProperty(ref _loadingBatterySlots_3F, value);
        }

        private ObservableCollection<LoadingBatterySlotViewModel> _loadingBatterySlots_2F = new ObservableCollection<LoadingBatterySlotViewModel>();
        public ObservableCollection<LoadingBatterySlotViewModel> LoadingBatterySlots_2F
        {
            get => _loadingBatterySlots_2F;
            set => SetProperty(ref _loadingBatterySlots_2F, value);
        }

        private ObservableCollection<LoadingBatterySlotViewModel> _loadingBatterySlots_1F = new ObservableCollection<LoadingBatterySlotViewModel>();
        public ObservableCollection<LoadingBatterySlotViewModel> LoadingBatterySlots_1F
        {
            get => _loadingBatterySlots_1F;
            set => SetProperty(ref _loadingBatterySlots_1F, value);
        }

        private LoadingBatterySlotViewModel _selectedLoadingBatterySlot = null;
        public LoadingBatterySlotViewModel SelectedLoadingBatterySlot
        {
            get => _selectedLoadingBatterySlot;
            set => SetProperty(ref _selectedLoadingBatterySlot, value);
        }

        private List<INV_WAREHOUSE> _outcomeWaitWarehouseInfos = new List<INV_WAREHOUSE>();
        private List<INV_WAREHOUSE> _loadingWarehouseInfos = new List<INV_WAREHOUSE>();

        private List<SlotInfo> _outcomeWaitSlotInfos = new List<SlotInfo>();
        private List<SlotInfo> _loadingSlotInfos = new List<SlotInfo>();

        public void Initialize()
        {
            ThreadStart threadStart = () =>
            {
                var envVariable = LibEnvVariable.GetEnvVariable();

                // DB 데이터 가져오기
                InitializeWarehouseInfos(envVariable.OUTCOME_WH_CD);
                InitializeWarehouseInfos(envVariable.LOADING_WH_CD);
                InitializeSlotInfos(envVariable.OUTCOME_WH_CD);
                InitializeSlotInfos(envVariable.LOADING_WH_CD);

                // 인스턴스 초기화
                InitializeOutcomeWaitBatteryInfos();
                InitializeLoadingBatteryInfos();
            };

            new PopupWaitViewModel()
            {
                Title = "출고 대기 장소 데이터 초기화",
            }.Open(threadStart);
        }

        private void InitializeWarehouseInfos(string WH_CD)
        {
            var envVariable = LibEnvVariable.GetEnvVariable();

            var query = new MLibMssql();
            query.Query(nameof(INV_WAREHOUSE));
            query.Select<INV_WAREHOUSE>(
                nameof(INV_WAREHOUSE.ROW),
                nameof(INV_WAREHOUSE.COL),
                nameof(INV_WAREHOUSE.LVL),
                nameof(INV_WAREHOUSE.LBL_ID));
            query.Where(nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.SITE_CD), $"{envVariable.SITE_CD}");
            query.Where(nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.FACT_CD), $"{envVariable.FACT_CD}");
            query.Where(nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.WH_CD), $"{WH_CD}");

            if (WH_CD == envVariable.OUTCOME_WH_CD)
            {
                _outcomeWaitWarehouseInfos = query.Get<INV_WAREHOUSE>();
            }
            else if (WH_CD == envVariable.LOADING_WH_CD)
            {
                _loadingWarehouseInfos = query.Get<INV_WAREHOUSE>();
            }
            else
            {
                throw new ArgumentException($"Unknown warehouse code: {WH_CD}");
            }
        }

        private void InitializeSlotInfos(string WH_CD)
        {
            var envVariable = LibEnvVariable.GetEnvVariable();

            var query = new MLibMssql();
            query.Query(nameof(INV_WAREHOUSE));
            query.Select("ROW_NUMBER() OVER (PARTITION BY [QLT_BTR_INSP].[LBL_ID] ORDER BY [QLT_BTR_INSP].[INSP_SEQ] DESC) as 'DU'");
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
            query.Where(nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.SITE_CD), $"{envVariable.SITE_CD}");
            query.Where(nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.FACT_CD), $"{envVariable.FACT_CD}");
            query.Where(nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.WH_CD), $"{WH_CD}");
            query.Where(nameof(QLT_BTR_INSP), nameof(QLT_BTR_INSP.BTR_DIAG_STS), EIF_FLAG.Y.ToString());
            query.Where(nameof(MST_BTR), nameof(MST_BTR.USE_YN), EYN_FLAG.Y.ToString());

            string selectQuery = query.GetSelectQuery();

            MLibMssql mainQuery = new MLibMssql();
            mainQuery.Query("LabelDuplication");
            mainQuery.With("LabelDuplication", selectQuery);
            mainQuery.Select("*");
            mainQuery.Where("LabelDuplication", "DU", 1);

            selectQuery = mainQuery.GetSelectQuery();
            if (WH_CD == envVariable.OUTCOME_WH_CD)
            {
                _outcomeWaitSlotInfos = mainQuery.Get<SlotInfo>();
            }
            else if (WH_CD == envVariable.LOADING_WH_CD)
            {
                _loadingSlotInfos = mainQuery.Get<SlotInfo>();
            }
            else
            {
                throw new ArgumentException($"Unknown warehouse code: {WH_CD}");
            }
        }

        private void InitializeOutcomeWaitBatteryInfos()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OutcomeWaitBatterySlots.Clear();
                OutcomeWaitBatterySlots_1F.Clear();
                OutcomeWaitBatterySlots_2F.Clear();
            });

            int batteryCount = 1;
            for (int row = 1; row <= RepoConstant.OutcomeWaitingRowCount; row++)
            {
                for (int level = 1; level <= RepoConstant.OutcomeWaitingLevelCount; level++)
                {
                    for (int column = 1; column <= RepoConstant.OutcomeWaitingColumnCount; column++)
                    {
                        var batterySlot = new OutcomeWaitBatterySlotViewModel();

                        var warehouseInfo = _outcomeWaitWarehouseInfos.Find(x => x.ROW == row.ToString() && x.COL == column.ToString() && x.LVL == level.ToString());
                        if (warehouseInfo == null) continue;

                        batterySlot.Row = int.Parse(warehouseInfo.ROW);
                        batterySlot.Column = int.Parse(warehouseInfo.COL);
                        batterySlot.Level = int.Parse(warehouseInfo.LVL);

                        batterySlot.BatteryInfo.Title = $"배터리 {batteryCount++}";

                        if (warehouseInfo.LBL_ID == string.Empty) // 라벨이 없을 경우
                        {
                            SetBatteryInfoVisibilityToEmpty(batterySlot);
                        }
                        else // 라벨이 있을 경우
                        {
                            var labelInfo = _outcomeWaitSlotInfos.Find(x => x.LBL_ID == warehouseInfo.LBL_ID);
                            if (labelInfo != null)
                            {
                                batterySlot.BatteryInfo.Label = labelInfo.LBL_ID;
                                batterySlot.BatteryInfo.CarManufacturer = labelInfo.CAR_MAKE_NM;
                                batterySlot.BatteryInfo.CarModel = labelInfo.CAR_NM;
                                batterySlot.BatteryInfo.BatteryManufacturer = labelInfo.BTR_MAKE_NM;
                                batterySlot.BatteryInfo.ReleaseYear = labelInfo.CAR_RELS_YEAR;
                                batterySlot.BatteryInfo.BatteryType = labelInfo.BTR_TYPE_NM;
                                batterySlot.BatteryInfo.PackOrModule = labelInfo.PACK_MDLE_CD;
                                batterySlot.BatteryInfo.Site = labelInfo.SITE_NM;
                                batterySlot.BatteryInfo.CollectionDate = labelInfo.COLT_DAT;
                                batterySlot.BatteryInfo.CollectionReason = labelInfo.COLT_RESN;
                                batterySlot.BatteryInfo.Grade = labelInfo.INSP_GRD;

                                SetBatteryInfoVisibilityToLabelExist(batterySlot);
                            }
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            OutcomeWaitBatterySlots.Add(batterySlot);
                            if (level == 1)
                            {
                                OutcomeWaitBatterySlots_1F.Add(batterySlot);
                            }
                            else if (level == 2)
                            {
                                OutcomeWaitBatterySlots_2F.Add(batterySlot);
                            }
                        });
                    }
                }
            }
        }

        private void InitializeLoadingBatteryInfos()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoadingBatterySlots.Clear();
                LoadingBatterySlots_1F.Clear();
                LoadingBatterySlots_2F.Clear();
                LoadingBatterySlots_3F.Clear();
            });

            for (int row = 1; row <= RepoConstant.LoadingRowCount; row++)
            {
                for (int level = 1; level <= RepoConstant.LoadingLevelCount; level++)
                {
                    for (int column = 1; column <= RepoConstant.LoadingColumnCount; column++)
                    {
                        var batterySlot = new LoadingBatterySlotViewModel();

                        var warehouseInfo = _loadingWarehouseInfos.Find(x => x.ROW == row.ToString() && x.COL == column.ToString() && x.LVL == level.ToString());
                        if (warehouseInfo == null) continue;

                        batterySlot.Row = int.Parse(warehouseInfo.ROW);
                        batterySlot.Column = int.Parse(warehouseInfo.COL);
                        batterySlot.Level = int.Parse(warehouseInfo.LVL);
                        batterySlot.BatteryInfo.FloorString = $"{batterySlot.Level}F";

                        if (warehouseInfo.LBL_ID != string.Empty) // 라벨이 있을 경우
                        {
                            var labelInfo = _loadingSlotInfos.Find(x => x.LBL_ID == warehouseInfo.LBL_ID);
                            if (labelInfo != null)
                            {
                                batterySlot.BatteryInfo.Visibility = Visibility.Visible;
                                batterySlot.BatteryInfo.Label = labelInfo.LBL_ID;
                                batterySlot.BatteryInfo.CarManufacturer = labelInfo.CAR_MAKE_NM;
                                batterySlot.BatteryInfo.CarModel = labelInfo.CAR_NM;
                                batterySlot.BatteryInfo.BatteryManufacturer = labelInfo.BTR_MAKE_NM;
                                batterySlot.BatteryInfo.ReleaseYear = labelInfo.CAR_RELS_YEAR;
                                batterySlot.BatteryInfo.BatteryType = labelInfo.BTR_TYPE_NM;
                                batterySlot.BatteryInfo.PackOrModule = labelInfo.PACK_MDLE_CD;
                                batterySlot.BatteryInfo.CollectionDate = labelInfo.COLT_DAT;
                                batterySlot.BatteryInfo.CollectionReason = labelInfo.COLT_RESN;
                                batterySlot.BatteryInfo.Grade = labelInfo.INSP_GRD;
                            }
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            LoadingBatterySlots.Add(batterySlot);
                            if (level == 1)
                            {
                                LoadingBatterySlots_1F.Add(batterySlot);
                            }
                            else if (level == 2)
                            {
                                LoadingBatterySlots_2F.Add(batterySlot);
                            }
                            else if (level == 3)
                            {
                                LoadingBatterySlots_3F.Add(batterySlot);
                            }
                        });
                    }
                }
            }

            // 역방향으로 저장 3F -> 2F -> 1F 순서
            var reverse = LoadingBatterySlots.Reverse().ToList();
            LoadingBatterySlots.Clear();
            LoadingBatterySlots.AddRange(reverse);

            // 상차장 배터리 슬롯 선택
            SelectTopLoadingBatterySlot();
        }

        public void SetBatteryInfoVisibilityToEmpty(OutcomeWaitBatterySlotViewModel batterySlot)
        {
            batterySlot.BatteryInfo.EmptyVisibility = Visibility.Visible;
            batterySlot.BatteryInfo.LabelExistVisibility = Visibility.Collapsed;
        }

        public void SetBatteryInfoVisibilityToLabelExist(OutcomeWaitBatterySlotViewModel batterySlot)
        {
            batterySlot.BatteryInfo.EmptyVisibility = Visibility.Collapsed;
            batterySlot.BatteryInfo.LabelExistVisibility = Visibility.Visible;
        }

        public void AddOutcomeWaitBattery(TransferInfoBinding requestStatus)
        {
            var batterySlot = OutcomeWaitBatterySlots.ToList().Find(x => x.Row == requestStatus.Row && x.Column == requestStatus.Column && x.Level == requestStatus.Level);

            var envVariable = LibEnvVariable.GetEnvVariable();

            // DB 업데이트
            MssqlUpdateQueryBuilder updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(INV_WAREHOUSE));
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.LBL_ID), requestStatus.LabelId);
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.LOAD_GRD), requestStatus.Grade);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.ROW), requestStatus.Row);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.COL), requestStatus.Column);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.LVL), requestStatus.Level);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.SITE_CD), envVariable.SITE_CD);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.FACT_CD), envVariable.FACT_CD);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.WH_CD), envVariable.OUTCOME_WH_CD);

            MssqlBasic.Update<INV_WAREHOUSE>(updateQueryBuilder);

            // BatteryInfo 데이터 업데이트
            batterySlot.BatteryInfo.Label = requestStatus.LabelId;
            batterySlot.BatteryInfo.CarManufacturer = requestStatus.CarManufacturer;
            batterySlot.BatteryInfo.CarModel = requestStatus.CarModel;
            batterySlot.BatteryInfo.BatteryManufacturer = requestStatus.BatteryManufacturer;
            batterySlot.BatteryInfo.ReleaseYear = requestStatus.ReleaseYear;
            batterySlot.BatteryInfo.BatteryType = requestStatus.BatteryType;
            batterySlot.BatteryInfo.PackOrModule = requestStatus.PackOrModule;
            batterySlot.BatteryInfo.Site = requestStatus.Site;
            batterySlot.BatteryInfo.CollectionDate = requestStatus.CollectionDate;
            batterySlot.BatteryInfo.CollectionReason = requestStatus.CollectionReason;
            batterySlot.BatteryInfo.Grade = requestStatus.Grade;

            SetBatteryInfoVisibilityToLabelExist(batterySlot);
        }

        public void AddLoadingBattery(OutcomeWaitBatteryViewModel.RequestStatusBinding requestStatus)
        {
            var batterySlot = LoadingBatterySlots.ToList().Find(x => x.Row == requestStatus.LoadingRow && x.Column == requestStatus.LoadingColumn && x.Level == requestStatus.LoadingLevel);

            var envVariable = LibEnvVariable.GetEnvVariable();

            // DB 업데이트
            MssqlUpdateQueryBuilder updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(INV_WAREHOUSE));
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.LBL_ID), requestStatus.Label);
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.LOAD_GRD), requestStatus.Grade);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.ROW), requestStatus.LoadingRow);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.COL), requestStatus.LoadingColumn);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.LVL), requestStatus.LoadingLevel);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.SITE_CD), envVariable.SITE_CD);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.FACT_CD), envVariable.FACT_CD);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.WH_CD), envVariable.LOADING_WH_CD);

            MssqlBasic.Update<INV_WAREHOUSE>(updateQueryBuilder);

            // BatteryInfo 데이터 업데이트
            batterySlot.BatteryInfo.Visibility = Visibility.Visible;
            batterySlot.BatteryInfo.Label = requestStatus.Label;
            batterySlot.BatteryInfo.CarManufacturer = requestStatus.CarManufacturer;
            batterySlot.BatteryInfo.CarModel = requestStatus.CarModel;
            batterySlot.BatteryInfo.BatteryManufacturer = requestStatus.BatteryManufacturer;
            batterySlot.BatteryInfo.ReleaseYear = requestStatus.ReleaseYear;
            batterySlot.BatteryInfo.BatteryType = requestStatus.BatteryType;
            batterySlot.BatteryInfo.PackOrModule = requestStatus.PackOrModule;
            batterySlot.BatteryInfo.Site = requestStatus.Site;
            batterySlot.BatteryInfo.CollectionDate = requestStatus.CollectionDate;
            batterySlot.BatteryInfo.CollectionReason = requestStatus.CollectionReason;
            batterySlot.BatteryInfo.Grade = requestStatus.Grade;

            // 상차장 배터리 슬롯 선택
            SelectTopLoadingBatterySlot();
        }

        public void AddLoadingBattery(DefectBatteryViewModel.TransferBatteryBinding requestStatus)
        {
            var batterySlot = LoadingBatterySlots.ToList().Find(x => x.Row == requestStatus.LoadingRow && x.Column == requestStatus.LoadingColumn && x.Level == requestStatus.LoadingLevel);

            var envVariable = LibEnvVariable.GetEnvVariable();

            // DB 업데이트
            MssqlUpdateQueryBuilder updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(INV_WAREHOUSE));
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.LBL_ID), requestStatus.Label);
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.LOAD_GRD), requestStatus.Grade);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.ROW), requestStatus.LoadingRow);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.COL), requestStatus.LoadingColumn);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.LVL), requestStatus.LoadingLevel);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.SITE_CD), envVariable.SITE_CD);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.FACT_CD), envVariable.FACT_CD);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.WH_CD), envVariable.LOADING_WH_CD);

            MssqlBasic.Update<INV_WAREHOUSE>(updateQueryBuilder);

            // BatteryInfo 데이터 업데이트
            batterySlot.BatteryInfo.Visibility = Visibility.Visible;
            batterySlot.BatteryInfo.Label = requestStatus.Label;
            batterySlot.BatteryInfo.CarManufacturer = requestStatus.CarManufacturer;
            batterySlot.BatteryInfo.CarModel = requestStatus.CarModel;
            batterySlot.BatteryInfo.BatteryManufacturer = requestStatus.BatteryManufacturer;
            batterySlot.BatteryInfo.ReleaseYear = requestStatus.ReleaseYear;
            batterySlot.BatteryInfo.BatteryType = requestStatus.BatteryType;
            batterySlot.BatteryInfo.PackOrModule = requestStatus.PackOrModule;
            batterySlot.BatteryInfo.Site = requestStatus.Site;
            batterySlot.BatteryInfo.CollectionDate = requestStatus.CollectionDate;
            batterySlot.BatteryInfo.CollectionReason = requestStatus.CollectionReason;
            batterySlot.BatteryInfo.Grade = requestStatus.Grade;

            // 상차장 배터리 슬롯 선택
            SelectTopLoadingBatterySlot();
        }

        public void SelectTopLoadingBatterySlot()
        {
            LoadingBatterySlotViewModel selectedSlot = null;
            foreach (var slot in LoadingBatterySlots)
            {
                if (slot.BatteryInfo.Label != string.Empty)
                {
                    selectedSlot = slot;
                    break;
                }
            }
            SelectedLoadingBatterySlot = selectedSlot;

            // UI 업데이트
            foreach (var slot in LoadingBatterySlots)
            {
                if (object.ReferenceEquals(slot, selectedSlot))
                {
                    selectedSlot.LoadingBatteryView.BorderThickness = new Thickness(2);
                    selectedSlot.LoadingBatteryView.BorderBrush = ResColor.border_action;
                }
                else
                {
                    slot.LoadingBatteryView.BorderThickness = new Thickness(1);
                    slot.LoadingBatteryView.BorderBrush = ResColor.border_primary;
                }
            }
        }

        public void DeleteOutcomeWaitBattery(OutcomeWaitBatterySlotViewModel batterySlot)
        {
            var envVariable = LibEnvVariable.GetEnvVariable();

            // DB 업데이트
            MssqlUpdateQueryBuilder updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(INV_WAREHOUSE));
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.LBL_ID), string.Empty);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.ROW), batterySlot.Row);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.COL), batterySlot.Column);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.LVL), batterySlot.Level);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.SITE_CD), envVariable.SITE_CD);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.FACT_CD), envVariable.FACT_CD);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.WH_CD), envVariable.OUTCOME_WH_CD);
            MssqlBasic.Update<INV_WAREHOUSE>(updateQueryBuilder);

            // BatteryInfo 데이터 업데이트
            batterySlot.BatteryInfo.Label = string.Empty;
            batterySlot.BatteryInfo.EmptyVisibility = Visibility.Visible;
            batterySlot.BatteryInfo.LabelExistVisibility = Visibility.Collapsed;
            batterySlot.BatteryInfo.TransferVisibility = Visibility.Collapsed;
        }
    }

    public class OutcomeWaitBatterySlotViewModel : BindableBase
    {
        public class TransferRequestViewBinding : BindableBase
        {
            private SolidColorBrush _borderBrush = ResColor.border_primary;
            public SolidColorBrush BorderBrush
            {
                get => _borderBrush;
                set => SetProperty(ref _borderBrush, value);
            }

            private Thickness _borderThickness = new Thickness(1);
            public Thickness BorderThickness
            {
                get => _borderThickness;
                set => SetProperty(ref _borderThickness, value);
            }

            private Visibility _ellipseVisibility = Visibility.Collapsed;
            public Visibility EllipseVisibility
            {
                get => _ellipseVisibility;
                set => SetProperty(ref _ellipseVisibility, value);
            }

            private int _transferOrder = 0;
            public int TransferOrder
            {
                get => _transferOrder;
                set => SetProperty(ref _transferOrder, value);
            }
        }

        public class BatteryInfoBinding : BindableBase
        {
            private string _title = string.Empty;
            public string Title
            {
                get => _title;
                set => SetProperty(ref _title, value);
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

            private Visibility _emptyVisibility = Visibility.Collapsed;
            public Visibility EmptyVisibility
            {
                get => _emptyVisibility;
                set => SetProperty(ref _emptyVisibility, value);
            }

            private Visibility _labelExistVisibility = Visibility.Collapsed;
            public Visibility LabelExistVisibility
            {
                get => _labelExistVisibility;
                set => SetProperty(ref _labelExistVisibility, value);
            }

            private Visibility _transferVisibility = Visibility.Collapsed;
            public Visibility TransferVisibility
            {
                get => _transferVisibility;
                set => SetProperty(ref _transferVisibility, value);
            }
        }

        public class LoadingPositionBinding : BindableBase
        {
            private int _row = 0;
            public int Row
            {
                get => _row;
                set => SetProperty(ref _row, value);
            }

            private int _column = 0;
            public int Column
            {
                get => _column;
                set => SetProperty(ref _column, value);
            }

            private int _level = 0;
            public int Level
            {
                get => _level;
                set => SetProperty(ref _level, value);
            }
        }

        private TransferRequestViewBinding _transferRequest = new TransferRequestViewBinding();
        public TransferRequestViewBinding TransferRequest
        {
            get => _transferRequest;
            set => SetProperty(ref _transferRequest, value);
        }

        private BatteryInfoBinding _batteryInfo = new BatteryInfoBinding();
        public BatteryInfoBinding BatteryInfo
        {
            get => _batteryInfo;
            set => SetProperty(ref _batteryInfo, value);
        }

        private LoadingPositionBinding _loadingPosition = new LoadingPositionBinding();
        public LoadingPositionBinding LoadingPosition
        {
            get => _loadingPosition;
            set => SetProperty(ref _loadingPosition, value);
        }

        private int _row = 0;
        public int Row
        {
            get => _row;
            set => SetProperty(ref _row, value);
        }

        private int _column = 0;
        public int Column
        {
            get => _column;
            set => SetProperty(ref _column, value);
        }

        private int _level = 0;
        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }
    }

    public class LoadingBatterySlotViewModel : BindableBase
    {
        public class LoadingBatteryViewBinding : BindableBase
        {
            private Thickness _borderThickness = new Thickness(1);
            public Thickness BorderThickness
            {
                get => _borderThickness;
                set => SetProperty(ref _borderThickness, value);
            }

            private SolidColorBrush _borderBrush = ResColor.border_primary;
            public SolidColorBrush BorderBrush
            {
                get => _borderBrush;
                set => SetProperty(ref _borderBrush, value);
            }
        }

        public class BatteryInfoBinding : BindableBase
        {
            private Visibility _visibility = Visibility.Collapsed;
            public Visibility Visibility
            {
                get => _visibility;
                set => SetProperty(ref _visibility, value);
            }

            private string _floorString = string.Empty;
            public string FloorString
            {
                get => _floorString;
                set => SetProperty(ref _floorString, value);
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
        }

        private LoadingBatteryViewBinding _loadingBatteryView = new LoadingBatteryViewBinding();
        public LoadingBatteryViewBinding LoadingBatteryView
        {
            get => _loadingBatteryView;
            set => SetProperty(ref _loadingBatteryView, value);
        }

        private BatteryInfoBinding _batteryInfo = new BatteryInfoBinding();
        public BatteryInfoBinding BatteryInfo
        {
            get => _batteryInfo;
            set => SetProperty(ref _batteryInfo, value);
        }

        private int _row = 0;
        public int Row
        {
            get => _row;
            set => SetProperty(ref _row, value);
        }

        private int _column = 0;
        public int Column
        {
            get => _column;
            set => SetProperty(ref _column, value);
        }

        private int _level = 0;
        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }
    }
}
