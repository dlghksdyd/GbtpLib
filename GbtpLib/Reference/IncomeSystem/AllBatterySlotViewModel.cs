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
using IncomeSystem.Design.Popup;
using IncomeSystem.Repository;
using MsSqlProcessor.MsSql;
using System.Collections.ObjectModel;
using System.Threading;
using IncomeSystem.Library;
using System.Windows.Media.Media3D;
using static MsSql.Utility.MsSqlProcedure;
using static IncomeSystem.Design.Common.BatterySlotViewModel;
using IncomeSystem.Design.LabelManagement;
using IncomeSystem.Design.IncomeInspection;
using System.Text.Encodings.Web;
using System.Text.Json;
using MsSqlProcessor.MintechSql;

namespace IncomeSystem.Design.Common
{
    public class AllBatterySlotViewModel : BindableBase
    {
        private static AllBatterySlotViewModel _instance = null;
        public static AllBatterySlotViewModel Instance
        {
            get => _instance ?? (_instance = new AllBatterySlotViewModel());
        }

        public class LabelInfo
        {
            // INV_WAREHOUSE
            public int ROW { get; set; } = 0;
            public int COL { get; set; } = 0;
            public int LVL { get; set; } = 0;
            public string LBL_ID { get; set; } = string.Empty;
            public string LOAD_GRD { get; set; } = string.Empty;
            public string STS { get; set; } = string.Empty;
            // MST_SITE
            public string SITE_NM { get; set; } = string.Empty;
            // MST_BTR
            public string BTR_TYPE_NO { get; set; } = string.Empty;
            public string COLT_DAT { get; set; } = string.Empty;
            public string COLT_RESN { get; set; } = string.Empty;
            public EYN_FLAG PRT_YN { get; set; } = EYN_FLAG.N;
            public string BTR_STS { get; set; } = string.Empty;
            public int MILE { get; set; } = 0;
            public string NOTE { get; set; } = string.Empty;
            public EYN_FLAG STO_INSP_FLAG { get; set; } = EYN_FLAG.N;
            // MST_BTR_TYPE
            public string BTR_TYPE_NM { get; set; } = string.Empty;
            public string CAR_MAKE_CD { get; set; } = string.Empty;
            public string CAR_CD { get; set; } = string.Empty;
            public string BTR_MAKE_CD { get; set; } = string.Empty;
            public string CAR_RELS_YEAR { get; set; } = string.Empty;
            public string PACK_MDLE_CD { get; set; } = string.Empty;
            public double VOLT_MAX_VALUE { get; set; } = 0.0;
            public double VOLT_MIN_VALUE { get; set; } = 0.0;
            public double ACIR_MAX_VALUE { get; set; } = 0.0;
            public double INSUL_MIN_VALUE { get; set; } = 0.0;

            // MST_CAR_MAKE
            public string CAR_MAKE_NM { get; set; } = string.Empty;
            // MST_CAR
            public string CAR_NM { get; set; } = string.Empty;
            // MST_BTR_MAKE
            public string BTR_MAKE_NM { get; set; } = string.Empty;
        }

        public class InspectionInfo : BindableBase
        {
            private bool _isSelected = false;
            public bool IsSelected
            {
                get => _isSelected;
                set => SetProperty(ref _isSelected, value);
            }

            // QLT_BTR_INOUT_INSP
            public string LBL_ID { get; set; } = string.Empty;
            public int INSP_SEQ { get; set; } = 0;
            public string INSP_VAL { get; set; } = string.Empty;
            public EInspectionResult INSP_RESULT { get; set; } = EInspectionResult.None;
            public string INSP_STA_DT { get; set; } = string.Empty;
            public string INSP_END_DT { get; set; } = string.Empty;
            public string NOTE { get; set; } = string.Empty;
            public string REG_ID { get; set; } = string.Empty;

            // MST_BTR
            public string COLT_DAT { get; set; } = string.Empty;

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

        private ObservableCollection<BatterySlotViewModel> _batterySlots = new ObservableCollection<BatterySlotViewModel>();
        public ObservableCollection<BatterySlotViewModel> BatterySlots
        {
            get => _batterySlots;
            set => SetProperty(ref _batterySlots, value);
        }

        private ObservableCollection<BatterySlotViewModel> _batterySlots_2F = new ObservableCollection<BatterySlotViewModel>();
        public ObservableCollection<BatterySlotViewModel> BatterySlots_2F
        {
            get => _batterySlots_2F;
            set => SetProperty(ref _batterySlots_2F, value);
        }

        private ObservableCollection<BatterySlotViewModel> _batterySlots_1F = new ObservableCollection<BatterySlotViewModel>();
        public ObservableCollection<BatterySlotViewModel> BatterySlots_1F
        {
            get => _batterySlots_1F;
            set => SetProperty(ref _batterySlots_1F, value);
        }

        private List<INV_WAREHOUSE> _warehouseInfos = new List<INV_WAREHOUSE>();
        private List<LabelInfo> _labelInfos = new List<LabelInfo>();

        private LabelManagementViewModel _labelManagementViewModel = null;
        private BatterySelectionViewModel _batterySelectionViewModel = null;

        public void Initialize()
        {
            ThreadStart threadStart = () =>
            {
                // DB 데이터 가져오기
                InitializeWarehouseInfos();
                InitializeLabelInfos();

                // 인스턴스 초기화
                InitializeBatteryInfos();
            };

            new PopupWaitViewModel()
            {
                Title = "입고 창고 데이터 초기화",
            }.Open(threadStart);
        }

        private void InitializeBatteryInfos()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                BatterySlots.Clear();
                BatterySlots_1F.Clear();
                BatterySlots_2F.Clear();
            });

            int batteryCount = 1;
            for (int row = 1; row <= RepoConstant.IncomeWaitingRowCount; row++)
            {
                for (int level = 1; level <= RepoConstant.IncomeWaitingLevelCount; level++)
                {
                    for (int column = 1; column <= RepoConstant.IncomeWaitingColumnCount; column++)
                    {
                        var batterySlot = new BatterySlotViewModel();

                        var warehouseInfo = _warehouseInfos.Find(x => x.ROW == row.ToString() && x.COL == column.ToString() && x.LVL == level.ToString());
                        if (warehouseInfo == null) continue;

                        batterySlot.Row = int.Parse(warehouseInfo.ROW);
                        batterySlot.Column = int.Parse(warehouseInfo.COL);
                        batterySlot.Level = int.Parse(warehouseInfo.LVL);

                        batterySlot.BatteryInfo.Title = $"배터리 {batteryCount++}";

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            batterySlot.BatteryInfo.PrintImage = new BitmapToImageSourceConverter().Convert(Images.IconImages.label_print_gray, null, null, null) as BitmapSource;
                        });

                        if (warehouseInfo.STORE_DIV == "02") // 배터리가 있을 경우
                        {
                            batterySlot.UnloadingZone.Text = "사용 중";
                            batterySlot.UnloadingZone.IsEnabled = false;
                            batterySlot.UnloadingZone.Background = ResColor.surface_disabled;
                            batterySlot.UnloadingZone.Foreground = ResColor.text_placeholder;

                            if (warehouseInfo.LBL_ID == string.Empty) // 라벨이 없을 경우
                            {
                                SetBatteryInfoVisibilityToLabelDontExist(batterySlot);
                            }
                            else // 라벨이 있을 경우
                            {
                                var labelInfo = _labelInfos.Find(x => x.LBL_ID == warehouseInfo.LBL_ID);
                                if (labelInfo != null)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        if (labelInfo.PRT_YN == EYN_FLAG.Y)
                                        {
                                            batterySlot.BatteryInfo.PrintImage = new BitmapToImageSourceConverter().Convert(Images.IconImages.label_print_color, null, null, null) as BitmapSource;
                                        }
                                    });

                                    batterySlot.BatteryInfo.Label = labelInfo.LBL_ID;
                                    batterySlot.BatteryInfo.CarManufacturer = labelInfo.CAR_MAKE_NM;
                                    batterySlot.BatteryInfo.CarModel = labelInfo.CAR_NM;
                                    batterySlot.BatteryInfo.BatteryManufacturer = labelInfo.BTR_MAKE_NM;
                                    batterySlot.BatteryInfo.ReleaseYear = labelInfo.CAR_RELS_YEAR;
                                    batterySlot.BatteryInfo.BatteryType = labelInfo.BTR_TYPE_NM;
                                    batterySlot.BatteryInfo.PackOrModule = labelInfo.PACK_MDLE_CD;
                                    batterySlot.BatteryInfo.VoltMaxValue = labelInfo.VOLT_MAX_VALUE;
                                    batterySlot.BatteryInfo.VoltMinValue = labelInfo.VOLT_MIN_VALUE;
                                    batterySlot.BatteryInfo.AcirMaxValue = labelInfo.ACIR_MAX_VALUE;
                                    batterySlot.BatteryInfo.InsulationMinValue = labelInfo.INSUL_MIN_VALUE;
                                    batterySlot.BatteryInfo.Site = labelInfo.SITE_NM;
                                    batterySlot.BatteryInfo.CollectionReason = labelInfo.COLT_RESN;
                                    batterySlot.BatteryInfo.CollectionDate = labelInfo.COLT_DAT;
                                    batterySlot.BatteryInfo.Mile = labelInfo.MILE;
                                    batterySlot.BatteryInfo.Note = labelInfo.NOTE;

                                    SetBatteryInfoVisibilityToLabelExist(batterySlot);
                                }

                                List<InspectionInfo> inspectionInfos = GetInspectionInfos(labelInfo.LBL_ID).Result;
                                if (inspectionInfos.Count >= 1) // 검사 결과가 있을 경우
                                {
                                    batterySlot.Inspection.InspectionFinishVisibility = System.Windows.Visibility.Visible;

                                    InspectionInfo inspectionInfo = inspectionInfos.First();
                                    batterySlot.InspectionInfo = JsonSerializer.Deserialize<InspectionInfoBinding>(inspectionInfo.INSP_VAL);
                                    batterySlot.InspectionInfo.InspectionResult = inspectionInfo.INSP_RESULT;
                                    batterySlot.InspectionInfo.InspectionSeq = inspectionInfo.INSP_SEQ;
                                }
                            }
                        }
                        else // 배터리가 없을 경우
                        {
                            batterySlot.UnloadingZone.Text = "빈 랙";
                            batterySlot.UnloadingZone.IsEnabled = true;
                            batterySlot.UnloadingZone.Background = ResColor.surface_page;
                            batterySlot.UnloadingZone.Foreground = ResColor.text_placeholder;

                            SetBatteryInfoVisibilityToEmpty(batterySlot);
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // 인스턴스 초기화
                            BatterySlots.Add(batterySlot);
                            if (level == 1)
                            {
                                BatterySlots_1F.Add(batterySlot);
                            }
                            else if (level == 2)
                            {
                                BatterySlots_2F.Add(batterySlot);
                            }
                        });
                    }
                }
            }
        }

        private void InitializeWarehouseInfos()
        {
            var selectQueryBuilder = new MssqlSelectQueryBuilder();
            selectQueryBuilder.AddAllColumns<INV_WAREHOUSE>();
            selectQueryBuilder.AddWhere(nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.WH_CD), $"{RepoConstant.IncomeWarehouseCode}");

            _warehouseInfos = MssqlBasic.Select<INV_WAREHOUSE>(nameof(INV_WAREHOUSE), selectQueryBuilder);
        }

        private void InitializeLabelInfos()
        {
            var selectQueryBuilder = new MssqlSelectQueryBuilder();
            selectQueryBuilder.AddSelectColumns<INV_WAREHOUSE>(
                new string[] {
                    nameof(INV_WAREHOUSE.ROW),
                    nameof(INV_WAREHOUSE.COL),
                    nameof(INV_WAREHOUSE.LVL),
                    nameof(INV_WAREHOUSE.LBL_ID),
                    nameof(INV_WAREHOUSE.LOAD_GRD),
                    nameof(INV_WAREHOUSE.STS),
                });
            selectQueryBuilder.AddSelectColumns<MST_SITE>(
                new string[] {
                    nameof(MST_SITE.SITE_NM),
                });
            selectQueryBuilder.AddSelectColumns<MST_BTR>(
                new string[] {
                    nameof(MST_BTR.BTR_TYPE_NO),
                    nameof(MST_BTR.COLT_DAT),
                    nameof(MST_BTR.COLT_RESN),
                    nameof(MST_BTR.PRT_YN),
                    nameof(MST_BTR.BTR_STS),
                    nameof(MST_BTR.MILE),
                    nameof(MST_BTR.NOTE),
                    nameof(MST_BTR.STO_INSP_FLAG),

                });
            selectQueryBuilder.AddSelectColumns<MST_BTR_TYPE>(
                new string[] {
                    nameof(MST_BTR_TYPE.BTR_TYPE_NM),
                    nameof(MST_BTR_TYPE.CAR_MAKE_CD),
                    nameof(MST_BTR_TYPE.CAR_CD),
                    nameof(MST_BTR_TYPE.BTR_MAKE_CD),
                    nameof(MST_BTR_TYPE.CAR_RELS_YEAR),
                    nameof(MST_BTR_TYPE.PACK_MDLE_CD),
                    nameof(MST_BTR_TYPE.VOLT_MAX_VALUE),
                    nameof(MST_BTR_TYPE.VOLT_MIN_VALUE),
                    nameof(MST_BTR_TYPE.ACIR_MAX_VALUE),
                    nameof(MST_BTR_TYPE.INSUL_MIN_VALUE),
                });
            selectQueryBuilder.AddSelectColumns<MST_CAR_MAKE>(
                new string[] {
                    nameof(MST_CAR_MAKE.CAR_MAKE_NM),
                });
            selectQueryBuilder.AddSelectColumns<MST_CAR>(
                new string[] {
                    nameof(MST_CAR.CAR_NM),
                });
            selectQueryBuilder.AddSelectColumns<MST_BTR_MAKE>(
                new string[] {
                    nameof(MST_BTR_MAKE.BTR_MAKE_NM),
                });
            selectQueryBuilder.AddLeftJoin(
                nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.LBL_ID),
                nameof(MST_BTR), nameof(MST_BTR.LBL_ID));
            selectQueryBuilder.AddLeftJoin(
                nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.SITE_CD),
                nameof(MST_SITE), nameof(MST_SITE.SITE_CD));
            selectQueryBuilder.AddLeftJoin(
                nameof(MST_BTR), nameof(MST_BTR.BTR_TYPE_NO),
                nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.BTR_TYPE_NO));
            selectQueryBuilder.AddLeftJoin(
                nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.CAR_MAKE_CD),
                nameof(MST_CAR_MAKE), nameof(MST_CAR_MAKE.CAR_MAKE_CD));
            selectQueryBuilder.AddLeftJoin(
                nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.CAR_CD),
                nameof(MST_CAR), nameof(MST_CAR.CAR_CD));
            selectQueryBuilder.AddLeftJoin(
                nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.BTR_MAKE_CD),
                nameof(MST_BTR_MAKE), nameof(MST_BTR_MAKE.BTR_MAKE_CD));
            selectQueryBuilder.AddWhere(
                nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.WH_CD), $"{RepoConstant.IncomeWarehouseCode}");
            selectQueryBuilder.AddWhereCondition_IsNotNull(
                nameof(INV_WAREHOUSE), nameof(INV_WAREHOUSE.LBL_ID));
            selectQueryBuilder.AddWhere(
                nameof(MST_BTR), nameof(MST_BTR.USE_YN), EYN_FLAG.Y.ToString());

            _labelInfos = MssqlBasic.Select<LabelInfo>(nameof(INV_WAREHOUSE), selectQueryBuilder);
        }

        public void SetBatteryInfoVisibilityToEmpty(BatterySlotViewModel batterySlot)
        {
            batterySlot.BatteryInfo.EmptyVisibility = Visibility.Visible;
            batterySlot.BatteryInfo.LabelDontExistVisibility = Visibility.Collapsed;
            batterySlot.BatteryInfo.LabelExistVisibility = Visibility.Collapsed;
        }

        public void SetBatteryInfoVisibilityToLabelDontExist(BatterySlotViewModel batterySlot)
        {
            batterySlot.BatteryInfo.EmptyVisibility = Visibility.Collapsed;
            batterySlot.BatteryInfo.LabelDontExistVisibility = Visibility.Visible;
            batterySlot.BatteryInfo.LabelExistVisibility = Visibility.Collapsed;
        }

        public void SetBatteryInfoVisibilityToLabelExist(BatterySlotViewModel batterySlot)
        {
            batterySlot.BatteryInfo.EmptyVisibility = Visibility.Collapsed;
            batterySlot.BatteryInfo.LabelDontExistVisibility = Visibility.Collapsed;
            batterySlot.BatteryInfo.LabelExistVisibility = Visibility.Visible;
        }

        public Task<List<InspectionInfo>> GetInspectionInfos(string labelId)
        {
            var envVariables = LibEnvVariable.GetEnvVariable();

            string SITE_CD = envVariables.SITE_CD;
            string FACT_CD = envVariables.FACT_CD;
            string PRCS_CD = envVariables.PRCS_CD;
            string MC_CD = envVariables.MC_CD;
            string INSP_KIND_GROUP_CD = envVariables.INSP_KIND_GROUP_CD;
            string INSP_KIND_CD = envVariables.INSP_KIND_CD;

            MLibMssql mLibMssql = new MLibMssql();
            return mLibMssql.Query(nameof(QLT_BTR_INOUT_INSP))
                .Select<QLT_BTR_INOUT_INSP>(
                    nameof(QLT_BTR_INOUT_INSP.LBL_ID),
                    nameof(QLT_BTR_INOUT_INSP.INSP_SEQ),
                    nameof(QLT_BTR_INOUT_INSP.INSP_VAL),
                    nameof(QLT_BTR_INOUT_INSP.INSP_RESULT),
                    nameof(QLT_BTR_INOUT_INSP.INSP_STA_DT),
                    nameof(QLT_BTR_INOUT_INSP.INSP_END_DT),
                    nameof(QLT_BTR_INOUT_INSP.NOTE),
                    nameof(QLT_BTR_INOUT_INSP.REG_ID)).
                Select<MST_BTR>(
                    nameof(MST_BTR.COLT_DAT)).
                Select<MST_BTR_TYPE>(
                    nameof(MST_BTR_TYPE.BTR_TYPE_NM),
                    nameof(MST_BTR_TYPE.CAR_RELS_YEAR)).
                Select<MST_CAR_MAKE>(
                    nameof(MST_CAR_MAKE.CAR_MAKE_NM)).
                Select<MST_CAR>(
                    nameof(MST_CAR.CAR_NM)).
                Select<MST_BTR_MAKE>(
                    nameof(MST_BTR_MAKE.BTR_MAKE_NM)).
                LeftJoin(
                    nameof(QLT_BTR_INOUT_INSP), nameof(QLT_BTR_INOUT_INSP.LBL_ID),
                    nameof(MST_BTR), nameof(MST_BTR.LBL_ID)).
                LeftJoin(
                    nameof(MST_BTR), nameof(MST_BTR.BTR_TYPE_NO),
                    nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.BTR_TYPE_NO)).
                LeftJoin(
                    nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.CAR_MAKE_CD),
                    nameof(MST_CAR_MAKE), nameof(MST_CAR_MAKE.CAR_MAKE_CD)).
                LeftJoin(
                    nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.CAR_CD),
                    nameof(MST_CAR), nameof(MST_CAR.CAR_CD)).
                LeftJoin(
                    nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.BTR_MAKE_CD),
                    nameof(MST_BTR_MAKE), nameof(MST_BTR_MAKE.BTR_MAKE_CD)).
                Where(nameof(QLT_BTR_INOUT_INSP), nameof(QLT_BTR_INOUT_INSP.LBL_ID), labelId).
                Where(nameof(QLT_BTR_INOUT_INSP), nameof(QLT_BTR_INOUT_INSP.SITE_CD), SITE_CD).
                Where(nameof(QLT_BTR_INOUT_INSP), nameof(QLT_BTR_INOUT_INSP.FACT_CD), FACT_CD).
                Where(nameof(QLT_BTR_INOUT_INSP), nameof(QLT_BTR_INOUT_INSP.PRCS_CD), PRCS_CD).
                Where(nameof(QLT_BTR_INOUT_INSP), nameof(QLT_BTR_INOUT_INSP.MC_CD), MC_CD).
                Where(nameof(QLT_BTR_INOUT_INSP), nameof(QLT_BTR_INOUT_INSP.INSP_KIND_GROUP_CD), INSP_KIND_GROUP_CD).
                Where(nameof(QLT_BTR_INOUT_INSP), nameof(QLT_BTR_INOUT_INSP.INSP_KIND_CD), INSP_KIND_CD).
                Where(nameof(MST_BTR), nameof(MST_BTR.USE_YN), EYN_FLAG.Y).
                OrderByDesc(nameof(QLT_BTR_INOUT_INSP), nameof(QLT_BTR_INOUT_INSP.INSP_SEQ)).
                GetAsync<InspectionInfo>();
        }

        public void AddInspectionResult(BatterySlotViewModel batterySlot)
        {
            var envVariables = LibEnvVariable.GetEnvVariable();
            string inspectionResult = JsonSerializer.Serialize(batterySlot.InspectionInfo, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

            MssqlInsertQueryBuilder insertQueryBuilder = new MssqlInsertQueryBuilder(nameof(QLT_BTR_INOUT_INSP));
            insertQueryBuilder.AddColumn(nameof(QLT_BTR_INOUT_INSP.SITE_CD), envVariables.SITE_CD);
            insertQueryBuilder.AddColumn(nameof(QLT_BTR_INOUT_INSP.FACT_CD), envVariables.FACT_CD);
            insertQueryBuilder.AddColumn(nameof(QLT_BTR_INOUT_INSP.PRCS_CD), envVariables.PRCS_CD);
            insertQueryBuilder.AddColumn(nameof(QLT_BTR_INOUT_INSP.MC_CD), envVariables.MC_CD);
            insertQueryBuilder.AddColumn(nameof(QLT_BTR_INOUT_INSP.INSP_KIND_GROUP_CD), envVariables.INSP_KIND_GROUP_CD);
            insertQueryBuilder.AddColumn(nameof(QLT_BTR_INOUT_INSP.INSP_KIND_CD), envVariables.INSP_KIND_CD);
            insertQueryBuilder.AddColumn(nameof(QLT_BTR_INOUT_INSP.LBL_ID), batterySlot.BatteryInfo.Label);
            insertQueryBuilder.AddColumn(nameof(QLT_BTR_INOUT_INSP.INSP_SEQ), (batterySlot.InspectionInfo.InspectionSeq).ToString());
            insertQueryBuilder.AddColumn(nameof(QLT_BTR_INOUT_INSP.INSP_VAL), inspectionResult);
            insertQueryBuilder.AddColumn(nameof(QLT_BTR_INOUT_INSP.INSP_RESULT), batterySlot.InspectionInfo.InspectionResult);
            insertQueryBuilder.AddColumn(nameof(QLT_BTR_INOUT_INSP.INSP_STA_DT), batterySlot.InspectionInfo.InspectionStartDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            insertQueryBuilder.AddColumn(nameof(QLT_BTR_INOUT_INSP.INSP_END_DT), batterySlot.InspectionInfo.InspectionEndDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            insertQueryBuilder.AddColumn(nameof(QLT_BTR_INOUT_INSP.NOTE), string.Empty);
            insertQueryBuilder.AddColumn(nameof(QLT_BTR_INOUT_INSP.REG_ID), RepoUser.UserId);
            insertQueryBuilder.AddColumn(nameof(QLT_BTR_INOUT_INSP.REG_DTM), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            MssqlBasic.Insert<QLT_BTR_INOUT_INSP>(insertQueryBuilder);
        }

        public void DeleteBattery(BatterySlotViewModel batterySlot)
        {
            // DB 업데이트
            MssqlUpdateQueryBuilder updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(INV_WAREHOUSE));
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.STORE_DIV), "00");
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.LBL_ID), string.Empty);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.ROW), batterySlot.Row);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.COL), batterySlot.Column);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.LVL), batterySlot.Level);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.WH_CD), RepoConstant.IncomeWarehouseCode);
            MssqlBasic.Update<INV_WAREHOUSE>(updateQueryBuilder);

            // UnloadingZone View 업데이트
            batterySlot.UnloadingZone.BorderThickness = new Thickness(1);
            batterySlot.UnloadingZone.BorderBrush = ResColor.border_primary;
            batterySlot.UnloadingZone.Text = "빈 랙";
            batterySlot.UnloadingZone.IsEnabled = true;
            batterySlot.UnloadingZone.Background = ResColor.surface_page;
            batterySlot.UnloadingZone.Foreground = ResColor.text_placeholder;

            // LabelManagement View 업데이트
            batterySlot.LabelManagement.BorderBrush = ResColor.border_primary;
            batterySlot.LabelManagement.BorderThickness = new Thickness(1);

            // IncomeInspection View 업데이트
            batterySlot.Inspection.BorderBrush = ResColor.border_primary;
            batterySlot.Inspection.BorderThickness = new Thickness(1);
            batterySlot.Inspection.InspectionFinishVisibility = Visibility.Collapsed;

            // TransferRequest View 업데이트
            batterySlot.TransferRequest.BorderBrush = ResColor.border_primary;
            batterySlot.TransferRequest.BorderThickness = new Thickness(1);
            batterySlot.TransferRequest.EllipseVisibility = Visibility.Collapsed;
            batterySlot.TransferRequest.TransferOrder = 0;

            // InspectionInfo 데이터 업데이트
            batterySlot.InspectionInfo.InspectionResult = EInspectionResult.None;
            batterySlot.InspectionInfo.InspectionSeq = 0;
            batterySlot.InspectionInfo.VisualResult = EInspectionResult.None;
            batterySlot.InspectionInfo.ElecResult = EInspectionResult.None;
            batterySlot.InspectionInfo.VoltResult = EInspectionResult.None;
            batterySlot.InspectionInfo.InsulResult = EInspectionResult.None;
            batterySlot.InspectionInfo.AcirResult = EInspectionResult.None;

            // BatteryInfo 데이터 업데이트
            batterySlot.BatteryInfo.Label = string.Empty;
            batterySlot.BatteryInfo.EmptyVisibility = Visibility.Visible;
            batterySlot.BatteryInfo.LabelDontExistVisibility = Visibility.Collapsed;
            batterySlot.BatteryInfo.LabelExistVisibility = Visibility.Collapsed;
            batterySlot.BatteryInfo.TransferVisibility = Visibility.Collapsed;
            Application.Current.Dispatcher.Invoke(() =>
            {
                batterySlot.BatteryInfo.PrintImage = new BitmapToImageSourceConverter().Convert(Images.IconImages.label_print_gray, null, null, null) as BitmapSource;
            });

            // LabelManagement UI 업데이트
            _labelManagementViewModel = LabelManagementViewModel.Instance;
            if (object.ReferenceEquals(_labelManagementViewModel.SelectedBatterySlot, batterySlot))
            {
                _labelManagementViewModel.ClearSlotSelection();
            }

            // _batterySelectionViewModel UI 업데이트
            _batterySelectionViewModel = BatterySelectionViewModel.Instance;
            if (Object.ReferenceEquals(_batterySelectionViewModel.SelectedBatterySlot, batterySlot))
            {
                _batterySelectionViewModel.ClearSlotSelection();
            }
        }

        public bool DeleteLabel(BatterySlotViewModel batterySlot)
        {
            // 입고 검사 했으면 삭제 불가
            if (batterySlot.InspectionInfo.InspectionResult != EInspectionResult.None)
            {
                new PopupWarningViewModel()
                {
                    Title = "라벨 삭제 실패",
                    Message = "입고 검사가 완료된 배터리는 라벨을 삭제할 수 없습니다.",
                    CancelButtonVisibility = Visibility.Collapsed
                }.Open();
                return false;
            }

            // DB 업데이트
            var deleteQueryBuilder = new MssqlDeleteQueryBuilder(nameof(MST_BTR));
            deleteQueryBuilder.AddWhere(nameof(MST_BTR.LBL_ID), batterySlot.BatteryInfo.Label);
            int result = MssqlBasic.Delete<MST_BTR>(deleteQueryBuilder);
            if (result < 1) return false;

            var updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(INV_WAREHOUSE));
            updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.LBL_ID), string.Empty);
            updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.LBL_ID), batterySlot.BatteryInfo.Label);
            MssqlBasic.Update<INV_WAREHOUSE>(updateQueryBuilder);

            // Slot 업데이트
            batterySlot.BatteryInfo.Label = string.Empty;
            batterySlot.BatteryInfo.EmptyVisibility = Visibility.Collapsed;
            batterySlot.BatteryInfo.LabelDontExistVisibility = Visibility.Visible;
            batterySlot.BatteryInfo.LabelExistVisibility = Visibility.Collapsed;
            batterySlot.BatteryInfo.PrintImage = new BitmapToImageSourceConverter().Convert(Images.IconImages.label_print_gray, null, null, null) as BitmapSource;

            // LabelManagement UI 업데이트
            _labelManagementViewModel = LabelManagementViewModel.Instance;
            if (object.ReferenceEquals(_labelManagementViewModel.SelectedBatterySlot, batterySlot))
            {
                _labelManagementViewModel.ClearSlotSelection();
            }

            // _batterySelectionViewModel UI 업데이트
            _batterySelectionViewModel = BatterySelectionViewModel.Instance;
            if (Object.ReferenceEquals(_batterySelectionViewModel.SelectedBatterySlot, batterySlot))
            {
                _batterySelectionViewModel.ClearSlotSelection();
            }

            return true;
        }
    }

    public class BatterySlotViewModel : BindableBase
    {
        public class UnloadingZoneViewBinding : BindableBase
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

            private SolidColorBrush _foreground = ResColor.text_placeholder;
            public SolidColorBrush Foreground
            {
                get => _foreground;
                set => SetProperty(ref _foreground, value);
            }

            private SolidColorBrush _background = ResColor.surface_page;
            public SolidColorBrush Background
            {
                get => _background;
                set => SetProperty(ref _background, value);
            }

            private bool _isEnabled = true;
            public bool IsEnabled
            {
                get => _isEnabled;
                set => SetProperty(ref _isEnabled, value);
            }

            private string _text = "빈 랙";
            public string Text
            {
                get => _text;
                set => SetProperty(ref _text, value);
            }
        }

        public class LabelManagementViewBinding : BindableBase
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
        }

        public class IncomeInspectionViewBinding : BindableBase
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

            private Visibility _inspectionFinishVisibility = Visibility.Collapsed;
            public Visibility InspectionFinishVisibility
            {
                get => _inspectionFinishVisibility;
                set => SetProperty(ref _inspectionFinishVisibility, value);
            }
        }

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

        public class InspectionInfoBinding : BindableBase
        {
            private EInspectionResult _inspectionResult = EInspectionResult.None;
            public EInspectionResult InspectionResult
            {
                get => _inspectionResult;
                set => SetProperty(ref _inspectionResult, value);
            }

            private int _inspectionSeq = 0;
            public int InspectionSeq
            {
                get => _inspectionSeq;
                set => SetProperty(ref _inspectionSeq, value);
            }

            private EInspectionResult _visualResult = EInspectionResult.None;
            public EInspectionResult VisualResult
            {
                get => _visualResult;
                set => SetProperty(ref _visualResult, value);
            }

            private string _visualComment = "-";
            public string VisualComment
            {
                get => _visualComment;
                set => SetProperty(ref _visualComment, value);
            }

            private EInspectionResult _elecResult = EInspectionResult.None;
            public EInspectionResult ElecResult
            {
                get => _elecResult;
                set => SetProperty(ref _elecResult, value);
            }

            private EInspectionResult _voltResult = EInspectionResult.None;
            public EInspectionResult VoltResult
            {
                get => _voltResult;
                set => SetProperty(ref _voltResult, value);
            }

            private string _voltVal = "-";
            public string VoltVal
            {
                get => _voltVal;
                set => SetProperty(ref _voltVal, value);
            }

            private EInspectionResult _insulResult = EInspectionResult.None;
            public EInspectionResult InsulResult
            {
                get => _insulResult;
                set => SetProperty(ref _insulResult, value);
            }

            private string _insulVal = "-";
            public string InsulVal
            {
                get => _insulVal;
                set => SetProperty(ref _insulVal, value);
            }

            private EInspectionResult _acirResult = EInspectionResult.None;
            public EInspectionResult AcirResult
            {
                get => _acirResult;
                set => SetProperty(ref _acirResult, value);
            }

            private string _acirVal = "-";
            public string AcirVal
            {
                get => _acirVal;
                set => SetProperty(ref _acirVal, value);
            }

            private DateTime _inspectionStartDateTime = DateTime.Now;
            public DateTime InspectionStartDateTime
            {
                get => _inspectionStartDateTime;
                set => SetProperty(ref _inspectionStartDateTime, value);
            }

            private DateTime _inspectionEndDateTime = DateTime.Now;
            public DateTime InspectionEndDateTime
            {
                get => _inspectionEndDateTime;
                set => SetProperty(ref _inspectionEndDateTime, value);
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

            private double _voltMaxValue = 0.0;
            public double VoltMaxValue
            {
                get => _voltMaxValue;
                set => SetProperty(ref _voltMaxValue, value);
            }

            private double _voltMinValue = 0.0;
            public double VoltMinValue
            {
                get => _voltMinValue;
                set => SetProperty(ref _voltMinValue, value);
            }

            private double _acirMaxValue = 0.0;
            public double AcirMaxValue
            {
                get => _acirMaxValue;
                set => SetProperty(ref _acirMaxValue, value);
            }

            private double _insulationMinValue = 0.0;
            public double InsulationMinValue
            {
                get => _insulationMinValue;
                set => SetProperty(ref _insulationMinValue, value);
            }

            private string _site = string.Empty;
            public string Site
            {
                get => _site;
                set => SetProperty(ref _site, value);
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

            private int _mile = 0;
            public int Mile
            {
                get => _mile;
                set => SetProperty(ref _mile, value);
            }

            private string _note = string.Empty;
            public string Note
            {
                get => _note;
                set => SetProperty(ref _note, value);
            }

            private BitmapSource _printImage = null;
            public BitmapSource PrintImage
            {
                get => _printImage;
                set => SetProperty(ref _printImage, value);
            }

            private Visibility _emptyVisibility = Visibility.Collapsed;
            public Visibility EmptyVisibility
            {
                get => _emptyVisibility;
                set => SetProperty(ref _emptyVisibility, value);
            }

            private Visibility _labelDontExistVisibility = Visibility.Collapsed;
            public Visibility LabelDontExistVisibility
            {
                get => _labelDontExistVisibility;
                set => SetProperty(ref _labelDontExistVisibility, value);
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

        private UnloadingZoneViewBinding _unloadingZone = new UnloadingZoneViewBinding();
        public UnloadingZoneViewBinding UnloadingZone
        {
            get => _unloadingZone;
            set => SetProperty(ref _unloadingZone, value);
        }

        private LabelManagementViewBinding _labelManagement = new LabelManagementViewBinding();
        public LabelManagementViewBinding LabelManagement
        {
            get => _labelManagement;
            set => SetProperty(ref _labelManagement, value);
        }

        private IncomeInspectionViewBinding _inspection = new IncomeInspectionViewBinding();
        public IncomeInspectionViewBinding Inspection
        {
            get => _inspection;
            set => SetProperty(ref _inspection, value);
        }

        private TransferRequestViewBinding _transferRequest = new TransferRequestViewBinding();
        public TransferRequestViewBinding TransferRequest
        {
            get => _transferRequest;
            set => SetProperty(ref _transferRequest, value);
        }

        private InspectionInfoBinding _inspectionInfo = new InspectionInfoBinding();
        public InspectionInfoBinding InspectionInfo
        {
            get => _inspectionInfo;
            set => SetProperty(ref _inspectionInfo, value);
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
