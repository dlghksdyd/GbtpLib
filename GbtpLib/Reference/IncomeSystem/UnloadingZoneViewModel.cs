using IncomeSystem.Design.Common;
using IncomeSystem.Design.Popup;
using IncomeSystem.Repository;
using MExpress.Mex;
using MsSqlProcessor.MsSql;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace IncomeSystem.Design.UnloadingZone
{
    public class UnloadingZoneViewModel : BindableBase
    {
        public class UnloadingAreaBinding : BindableBase
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

            private string _text = string.Empty;
            public string Text
            {
                get => _text;
                set => SetProperty(ref _text, value);
            }

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

        public class IncomeWaitingAreaBinding : BindableBase
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

            private bool _isEnabled = false;
            public bool IsEnabled
            {
                get => _isEnabled;
                set => SetProperty(ref _isEnabled, value);
            }

            private string _text = string.Empty;
            public string Text
            {
                get => _text;
                set => SetProperty(ref _text, value);
            }
        }

        public class RequestStatusBinding : BindableBase
        {
            private ERequestStatus _statusEnum = ERequestStatus.Waiting;
            public ERequestStatus StatusEnum
            {
                get => _statusEnum;
                set => SetProperty(ref _statusEnum, value);
            }

            private int _unloadingRow = 0;
            public int UnloadingRow
            {
                get => _unloadingRow;
                set => SetProperty(ref _unloadingRow, value);
            }

            private int _unloadingColumn = 0;
            public int UnloadingColumn
            {
                get => _unloadingColumn;
                set => SetProperty(ref _unloadingColumn, value);
            }

            private int _unloadingLevel = 0;
            public int UnloadingLevel
            {
                get => _unloadingLevel;
                set => SetProperty(ref _unloadingLevel, value);
            }

            private int _incomeWaitingRow = 0;
            public int IncomeWaitingRow
            {
                get => _incomeWaitingRow;
                set => SetProperty(ref _incomeWaitingRow, value);
            }

            private int _incomeWaitingColumn = 0;
            public int IncomeWaitingColumn
            {
                get => _incomeWaitingColumn;
                set => SetProperty(ref _incomeWaitingColumn, value);
            }

            private int _incomeWaitingLevel = 0;
            public int IncomeWaitingLevel
            {
                get => _incomeWaitingLevel;
                set => SetProperty(ref _incomeWaitingLevel, value);
            }

            private int _order = 0;
            public int Order
            {
                get => _order;
                set => SetProperty(ref _order, value);
            }

            private string _position = string.Empty;
            public string Position
            {
                get => _position;
                set => SetProperty(ref _position, value);
            }

            private string _status = string.Empty;
            public string StatusText
            {
                get => _status;
                set => SetProperty(ref _status, value);
            }

            private Visibility _cancelImageVisibility = Visibility.Collapsed;
            public Visibility CancelImageVisibility
            {
                get => _cancelImageVisibility;
                set => SetProperty(ref _cancelImageVisibility, value);
            }
        }

        private static UnloadingZoneViewModel _instance = null;
        public static UnloadingZoneViewModel Instance
        {
            get => _instance ?? (_instance = new UnloadingZoneViewModel());
        }

        private ObservableCollection<UnloadingAreaBinding> _unloadingAreas = new ObservableCollection<UnloadingAreaBinding>();
        public ObservableCollection<UnloadingAreaBinding> UnloadingAreas
        {
            get => _unloadingAreas;
            set => SetProperty(ref _unloadingAreas, value);
        }

        private BatterySlotViewModel _batteryDontExistArea = null;
        public BatterySlotViewModel BatteryDontExistArea
        {
            get => _batteryDontExistArea;
            set => SetProperty(ref _batteryDontExistArea, value);
        }

        private ObservableCollection<RequestStatusBinding> _requestStatuses = new ObservableCollection<RequestStatusBinding>();
        public ObservableCollection<RequestStatusBinding> RequestStatuses
        {
            get => _requestStatuses;
            set => SetProperty(ref _requestStatuses, value);
        }

        private bool _isCountSetButtonEnabled = true;
        public bool IsCountSetButtonEnabled
        {
            get => _isCountSetButtonEnabled;
            set => SetProperty(ref _isCountSetButtonEnabled, value);
        }

        public enum ERequestStatus
        {
            Waiting,
            InProgress,
            Finished,
        }

        private AllBatterySlotViewModel _allBatterySlotViewModel = null;

        private UnloadingAreaBinding _selectedUnloadingArea = new UnloadingAreaBinding();
        private BatterySlotViewModel _selectedIncomeWaitingArea = new BatterySlotViewModel();

        private Thread _checkCommandThread = null;

        private object _requestStatusLock = new object();

        public UnloadingZoneViewModel()
        {
            InitializeUnloadingArea();
            InitializeIncomeWaitingArea();
        }

        public void Loaded()
        {
            if (_checkCommandThread == null)
            {
                _checkCommandThread = new Thread(CheckCommandThread);
                _checkCommandThread.IsBackground = true;
                _checkCommandThread.Start();
            }

            if (_allBatterySlotViewModel == null)
            {
                _allBatterySlotViewModel = AllBatterySlotViewModel.Instance;
            }
        }

        private void CheckCommandThread()
        {
            while (true)
            {
                Thread.Sleep(2000);

                // 전송 중인 배터리가 없을 경우
                if (RequestStatuses.Count == 0)
                {
                    continue;
                }

                var requestStatus = RequestStatuses.First();

                if (requestStatus.StatusEnum == ERequestStatus.Waiting)
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@IN_CMD_CD", EIF_CMD.AA0.ToString() },
                        { "@IN_DATA1", string.Empty },
                        { "@IN_DATA2", requestStatus.UnloadingRow },
                        { "@IN_DATA3", requestStatus.UnloadingColumn },
                        { "@IN_DATA4", requestStatus.UnloadingLevel },
                        { "@IN_DATA5", requestStatus.IncomeWaitingRow },
                        { "@IN_DATA6", requestStatus.IncomeWaitingColumn },
                        { "@IN_DATA7", requestStatus.IncomeWaitingLevel },
                        { "@IN_REQ_SYS", RepoConstant.SystemCodeIncome }
                    };
                    MssqlBasic.ExecuteStoredProcedure("BRDS_ITF_CMD_DATA_SET", parameters);

                    requestStatus.StatusText = "이송 중";
                    requestStatus.StatusEnum = ERequestStatus.InProgress;
                    requestStatus.CancelImageVisibility = Visibility.Hidden;
                }
                else if (requestStatus.StatusEnum == ERequestStatus.InProgress)
                {
                    MssqlSelectQueryBuilder selectQueryBuilder = new MssqlSelectQueryBuilder();
                    selectQueryBuilder.AddAllColumns<ITF_CMD_DATA>();
                    selectQueryBuilder.AddWhere(
                        nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.CMD_CD), EIF_CMD.AA1.ToString());
                    selectQueryBuilder.AddWhere(
                        nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.IF_FLAG), EIF_FLAG.C.ToString());
                    var cmdData_AA1 = MssqlBasic.Select<ITF_CMD_DATA>(nameof(ITF_CMD_DATA), selectQueryBuilder);

                    if (cmdData_AA1.Count > 0)
                    {
                        MssqlUpdateQueryBuilder updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(ITF_CMD_DATA));
                        updateQueryBuilder.AddSet(nameof(ITF_CMD_DATA.IF_FLAG), EIF_FLAG.Y.ToString());
                        updateQueryBuilder.AddSet(nameof(ITF_CMD_DATA.RES_TIME), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        updateQueryBuilder.AddSet(nameof(ITF_CMD_DATA.RES_SYS), RepoConstant.SystemCodeIncome);
                        updateQueryBuilder.AddWhere(nameof(ITF_CMD_DATA.CMD_CD), EIF_CMD.AA1.ToString());
                        int result = MssqlBasic.Update<ITF_CMD_DATA>(updateQueryBuilder);
                        if (result > 0)
                        {
                            requestStatus.StatusText = "이송 완료";
                            requestStatus.StatusEnum = ERequestStatus.Finished;
                        }
                    }
                }
                else if (requestStatus.StatusEnum == ERequestStatus.Finished)
                {
                    // DB 업데이트
                    MssqlUpdateQueryBuilder updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(INV_WAREHOUSE));
                    updateQueryBuilder.AddSet(nameof(INV_WAREHOUSE.STORE_DIV), "02"); // 입고 대기
                    updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.ROW), requestStatus.IncomeWaitingRow);
                    updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.COL), requestStatus.IncomeWaitingColumn);
                    updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.LVL), requestStatus.IncomeWaitingLevel);
                    updateQueryBuilder.AddWhere(nameof(INV_WAREHOUSE.WH_CD), RepoConstant.IncomeWarehouseCode);
                    int result = MssqlBasic.Update<ITF_CMD_DATA>(updateQueryBuilder);
                    if (result >= 0)
                    {
                        var batterySlot = _allBatterySlotViewModel.BatterySlots.ToList().Find(x =>
                            x.Row == requestStatus.IncomeWaitingRow &&
                            x.Column == requestStatus.IncomeWaitingColumn &&
                            x.Level == requestStatus.IncomeWaitingLevel);
                        _allBatterySlotViewModel.SetBatteryInfoVisibilityToLabelDontExist(batterySlot);

                        SetIncomeWaitingAreaToUse(batterySlot);

                        lock (_requestStatusLock)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                RequestStatuses.RemoveAt(0);
                            });

                            UpdateTransferingOrder();
                        }

                        // 모든 요청이 끝남 & 모든 이송 완료
                        if (_selectedUnloadingArea.Text == string.Empty && RequestStatuses.Count == 0)
                        {
                            // 하차장 선택 초기화
                            SelectUnloadingArea(RepoConstant.UnloadingLevelCount);
                        }
                    }
                }
            }
        }

        private void InitializeUnloadingArea()
        {
            for (int i = RepoConstant.UnloadingLevelCount; i > 0; i--)
            {
                UnloadingAreaBinding unloadingZone = new UnloadingAreaBinding
                {
                    Row = 1,
                    Column = 1,
                    Level = i,
                    Text = $"{i}F",
                    BorderBrush = ResColor.border_primary,
                    BorderThickness = new Thickness(1)
                };
                UnloadingAreas.Add(unloadingZone);
            }

            SelectUnloadingArea(RepoConstant.UnloadingLevelCount);
        }

        private void InitializeIncomeWaitingArea()
        {
            BatteryDontExistArea = new BatterySlotViewModel();
            BatteryDontExistArea.Row = 0;
            BatteryDontExistArea.Column = 0;
            BatteryDontExistArea.Level = 0;
            BatteryDontExistArea.UnloadingZone.BorderBrush = ResColor.border_primary;
            BatteryDontExistArea.UnloadingZone.BorderThickness = new Thickness(1);
            BatteryDontExistArea.UnloadingZone.Text = "배터리 없음";
        }

        private void SetIncomeWaitingAreaToEmpty(BatterySlotViewModel slot)
        {
            slot.UnloadingZone.Text = "빈 랙";
            slot.UnloadingZone.Background = ResColor.surface_page;
            slot.UnloadingZone.Foreground = ResColor.text_placeholder;
            slot.UnloadingZone.IsEnabled = true;
        }

        private void SetIncomeWaitingAreaToUse(BatterySlotViewModel slot)
        {
            slot.UnloadingZone.Text = "사용중";
            slot.UnloadingZone.Background = ResColor.surface_disabled;
            slot.UnloadingZone.Foreground = ResColor.text_placeholder;
            slot.UnloadingZone.IsEnabled = false;
        }

        private void SetIncomeWaitingAreaToTransfering(BatterySlotViewModel slot)
        {
            slot.UnloadingZone.Text = "이송 중";
            slot.UnloadingZone.Background = ResColor.surface_disabled;
            slot.UnloadingZone.Foreground = ResColor.text_placeholder;
            slot.UnloadingZone.IsEnabled = false;
        }

        public void SelectUnloadingArea(int level)
        {
            foreach (var area in UnloadingAreas)
            {
                if (area.Level == level)
                {
                    area.BorderBrush = ResColor.border_action;
                    area.BorderThickness = new Thickness(2);

                    _selectedUnloadingArea = area;
                }
                else
                {
                    area.BorderBrush = ResColor.border_primary;
                    area.BorderThickness = new Thickness(1);
                }
            }

            if (level == 0)
            {
                _selectedUnloadingArea = new UnloadingAreaBinding();
            }
        }

        private void DownUnloadingArea()
        {
            if (_selectedUnloadingArea.Level > 0)
            {
                SelectUnloadingArea(_selectedUnloadingArea.Level - 1);
            }
        }

        public bool IsTransferingBatteryExist()
        {
            return RequestStatuses.Count > 0;
        }

        public void SelectIncomeWaitingArea(int row, int column, int level)
        {
            foreach (var area in _allBatterySlotViewModel.BatterySlots)
            {
                if (area.Row == row && area.Column == column && area.Level == level)
                {
                    area.UnloadingZone.BorderBrush = ResColor.border_action;
                    area.UnloadingZone.BorderThickness = new Thickness(2);

                    _selectedIncomeWaitingArea = area;
                }
                else
                {
                    area.UnloadingZone.BorderBrush = ResColor.border_primary;
                    area.UnloadingZone.BorderThickness = new Thickness(1);
                }
            }

            if (row == 0 && column == 0 && level == 0)
            {
                BatteryDontExistArea.UnloadingZone.BorderThickness = new Thickness(2);
                BatteryDontExistArea.UnloadingZone.BorderBrush = ResColor.border_action;

                _selectedIncomeWaitingArea = BatteryDontExistArea;
            }
            else
            {
                BatteryDontExistArea.UnloadingZone.BorderThickness = new Thickness(1);
                BatteryDontExistArea.UnloadingZone.BorderBrush = ResColor.border_primary;
            }
        }

        public void UnselectIncomeWaitingArea()
        {
            foreach (var area in _allBatterySlotViewModel.BatterySlots)
            {
                area.UnloadingZone.BorderBrush = ResColor.border_primary;
                area.UnloadingZone.BorderThickness = new Thickness(1);
            }
            BatteryDontExistArea.UnloadingZone.BorderBrush = ResColor.border_primary;
            BatteryDontExistArea.UnloadingZone.BorderThickness = new Thickness(1);
            _selectedIncomeWaitingArea = new BatterySlotViewModel();
            _selectedIncomeWaitingArea.UnloadingZone.Text = string.Empty;
        }

        public void TransferBattery()
        {
            if (_selectedUnloadingArea.Text == string.Empty)
            {
                new PopupWarningViewModel()
                {
                    Title = "하차장",
                    Message = "모든 하차장의 배터리가 이송 중입니다.",
                    CancelButtonVisibility = Visibility.Collapsed
                }.Open();
                return;
            }

            if (_selectedIncomeWaitingArea.UnloadingZone.Text == string.Empty)
            {
                new PopupWarningViewModel()
                {
                    Title = "입고 대기 장소 위치 선택",
                    Message = "입고 대기 장소 위치를 선택해주세요.",
                    CancelButtonVisibility = Visibility.Collapsed
                }.Open();
                return;
            }

            if (_selectedIncomeWaitingArea.Row > 0)
            {
                var batterySlot = _allBatterySlotViewModel.BatterySlots.ToList().Find(x =>
                    x.Row == _selectedIncomeWaitingArea.Row &&
                    x.Column == _selectedIncomeWaitingArea.Column &&
                    x.Level == _selectedIncomeWaitingArea.Level);

                SetIncomeWaitingAreaToTransfering(batterySlot);

                lock (_requestStatusLock)
                {
                    RequestStatusBinding requestStatus = new RequestStatusBinding();
                    requestStatus.UnloadingRow = _selectedUnloadingArea.Row;
                    requestStatus.UnloadingColumn = _selectedUnloadingArea.Column;
                    requestStatus.UnloadingLevel = _selectedUnloadingArea.Level;
                    requestStatus.IncomeWaitingRow = _selectedIncomeWaitingArea.Row;
                    requestStatus.IncomeWaitingColumn = _selectedIncomeWaitingArea.Column;
                    requestStatus.IncomeWaitingLevel = _selectedIncomeWaitingArea.Level;
                    requestStatus.StatusEnum = ERequestStatus.Waiting;
                    requestStatus.StatusText = "대기 중";
                    requestStatus.Order = RequestStatuses.Count + 1;
                    requestStatus.Position = $"{_selectedIncomeWaitingArea.Level}F-{_selectedIncomeWaitingArea.Column}";
                    requestStatus.CancelImageVisibility = Visibility.Visible;
                    RequestStatuses.Add(requestStatus);
                }
            }

            UnselectIncomeWaitingArea();
            DownUnloadingArea();
        }

        public void CancelTransfering(RequestStatusBinding requestStatus)
        {
            int index = RequestStatuses.ToList().FindIndex(x => ReferenceEquals(x, requestStatus));

            SelectUnloadingArea(requestStatus.UnloadingLevel);

            for (int i = index; i < RequestStatuses.Count; i++)
            {
                var oneStatus = RequestStatuses[i];

                var batterySlot = _allBatterySlotViewModel.BatterySlots.ToList().Find(x =>
                    x.Row == oneStatus.IncomeWaitingRow &&
                    x.Column == oneStatus.IncomeWaitingColumn &&
                    x.Level == oneStatus.IncomeWaitingLevel);
                SetIncomeWaitingAreaToEmpty(batterySlot);
            }

            for (int i = 0; i < RepoConstant.UnloadingLevelCount; i++)
            {
                if (RequestStatuses.Count > index)
                {
                    lock (_requestStatusLock)
                    {
                        RequestStatuses.RemoveAt(index);
                    }
                }
            }

            UpdateTransferingOrder();
        }

        private void UpdateTransferingOrder()
        {
            for (int i = 0; i < RequestStatuses.Count; i++)
            {
                RequestStatuses[i].Order = i + 1;
            }
        }
    }
}
