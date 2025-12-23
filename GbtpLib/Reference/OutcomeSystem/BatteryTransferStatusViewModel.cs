using MExpress.Mex;
using MsSqlProcessor.MsSql;
using OutcomeSystem.Design.Common;
using OutcomeSystem.Design.GradeClassBatterySearch;
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
using System.Windows.Data;
using System.Windows.Media;

namespace OutcomeSystem.Design.BatteryTransferStatus
{
    public class BatteryTransferStatusViewModel : BindableBase
    {
        private static BatteryTransferStatusViewModel _instance;
        public static BatteryTransferStatusViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BatteryTransferStatusViewModel();
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

        private ObservableCollection<TransferInfoBinding> _requestStatuses = new ObservableCollection<TransferInfoBinding>();
        public ObservableCollection<TransferInfoBinding> RequestStatuses
        {
            get => _requestStatuses;
            set
            {
                SetProperty(ref _requestStatuses, value);
            }
        }

        private AllBatterySlotViewModel _allBatterySlotViewModel = null;
        private GradeClassBatterySearchViewModel _gradeClassBatterySearchViewModel = null;

        private Thread _transferThread = null;
        private object _transferLock = new object();

        public BatteryTransferStatusViewModel()
        {
            if (_transferThread == null)
            {
                _transferThread = new Thread(TransferThread);
                _transferThread.IsBackground = true;
                _transferThread.Start();
            }

            if (_allBatterySlotViewModel == null)
            {
                _allBatterySlotViewModel = AllBatterySlotViewModel.Instance;
            }
        }

        public void Loaded()
        {
            if (_gradeClassBatterySearchViewModel == null)
            {
                _gradeClassBatterySearchViewModel = GradeClassBatterySearchViewModel.Instance;
            }
        }

        private void TransferThread()
        {
            while (true)
            {
                Thread.Sleep(1000);

                lock (_transferLock)
                {
                    if (RequestStatuses.Count == 0) continue;
                    var requestStatus = RequestStatuses.First();

                    if (requestStatus.StatusEnum == ERequestStatus.Waiting)
                    {
                        var parameters = new Dictionary<string, object>
                    {
                        { "@IN_CMD_CD", EIF_CMD.EE1.ToString() },
                        { "@IN_DATA1", requestStatus.LabelId },
                        { "@IN_DATA2", requestStatus.Row },
                        { "@IN_DATA3", requestStatus.Column },
                        { "@IN_DATA4", requestStatus.Level },
                        { "@IN_REQ_SYS", RepoConstant.SystemCodeOutcome }
                    };
                        bool result = MssqlBasic.ExecuteStoredProcedure("BRDS_ITF_CMD_DATA_SET", parameters);
                        if (result)
                        {
                            requestStatus.StatusText = "이송 중";
                            requestStatus.StatusEnum = ERequestStatus.InProgress;
                            requestStatus.CancelButtonVisibility = Visibility.Hidden;
                            requestStatus.StatusVisibility = Visibility.Visible;
                        }
                    }
                    else if (requestStatus.StatusEnum == ERequestStatus.InProgress)
                    {
                        var selectQueryBuilder = new MssqlSelectQueryBuilder();
                        selectQueryBuilder.AddSelectColumns<ITF_CMD_DATA>(new string[]
                        {
                        nameof(ITF_CMD_DATA.CMD_CD),
                        });
                        selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.DATA1), requestStatus.LabelId);
                        selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.CMD_CD), EIF_CMD.EE2.ToString());
                        selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.IF_FLAG), EIF_FLAG.C.ToString());
                        var itfCmdDataList = MssqlBasic.Select<ITF_CMD_DATA>(nameof(ITF_CMD_DATA), selectQueryBuilder);
                        if (itfCmdDataList.Count >= 1)
                        {
                            var itfCmdData = itfCmdDataList.First();
                            var updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(ITF_CMD_DATA));
                            updateQueryBuilder.AddSet(nameof(ITF_CMD_DATA.IF_FLAG), EIF_FLAG.Y);
                            updateQueryBuilder.AddWhere(nameof(ITF_CMD_DATA.CMD_CD), EIF_CMD.EE2.ToString());
                            updateQueryBuilder.AddWhere(nameof(ITF_CMD_DATA.DATA1), requestStatus.LabelId);
                            int result = MssqlBasic.Update<ITF_CMD_DATA>(updateQueryBuilder);
                            if (result >= 1)
                            {
                                requestStatus.StatusText = "이송 완료";
                                requestStatus.StatusEnum = ERequestStatus.Finished;
                            }
                        }
                    }
                    else if (requestStatus.StatusEnum == ERequestStatus.Finished)
                    {
                        // 인스턴스 업데이트
                        _allBatterySlotViewModel.AddOutcomeWaitBattery(requestStatus);

                        // UI 업데이트
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            RequestStatuses.Remove(requestStatus);
                        });
                    }
                }
            }
        }

        public void CancelRequestStatus(TransferInfoBinding requestStatus)
        {
            // 등급 분류 창고 검사 페이지 UI 원상 복구
            _gradeClassBatterySearchViewModel.ReturnBattery(requestStatus);

            // 전송 중인 배터리 삭제
            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (_transferLock)
                {
                    RequestStatuses.Remove(requestStatus);
                }
            });
        }
    }

    public class TransferInfoBinding : BindableBase
    {
        public class OutcomeSlotBinding : BindableBase
        {
            private SolidColorBrush _background = ResColor.surface_secondary;
            public SolidColorBrush Background
            {
                get => _background;
                set
                {
                    SetProperty(ref _background, value);
                }
            }
        }

        private int _row = 0;
        public int Row
        {
            get => _row;
            set
            {
                SetProperty(ref _row, value);
            }
        }

        private int _level = 0;
        public int Level
        {
            get => _level;
            set
            {
                SetProperty(ref _level, value);
            }
        }

        private int _column = 0;
        public int Column
        {
            get => _column;
            set
            {
                SetProperty(ref _column, value);
            }
        }

        private string _labelId = string.Empty;
        public string LabelId
        {
            get => _labelId;
            set
            {
                SetProperty(ref _labelId, value);
            }
        }

        private string _carManufacturer = string.Empty;
        public string CarManufacturer
        {
            get => _carManufacturer;
            set
            {
                SetProperty(ref _carManufacturer, value);
            }
        }

        private string _carModel = string.Empty;
        public string CarModel
        {
            get => _carModel;
            set
            {
                SetProperty(ref _carModel, value);
            }
        }

        private string _releaseYear = string.Empty;
        public string ReleaseYear
        {
            get => _releaseYear;
            set
            {
                SetProperty(ref _releaseYear, value);
            }
        }

        private string _grade = string.Empty;
        public string Grade
        {
            get => _grade;
            set
            {
                SetProperty(ref _grade, value);
            }
        }

        private string _collectionDate = string.Empty;
        public string CollectionDate
        {
            get => _collectionDate;
            set
            {
                SetProperty(ref _collectionDate, value);
            }
        }

        private string _collectionReason = string.Empty;
        public string CollectionReason
        {
            get => _collectionReason;
            set
            {
                SetProperty(ref _collectionReason, value);
            }
        }

        private string _packOrModule = string.Empty;
        public string PackOrModule
        {
            get => _packOrModule;
            set
            {
                SetProperty(ref _packOrModule, value);
            }
        }

        private string _site = string.Empty;
        public string Site
        {
            get => _site;
            set
            {
                SetProperty(ref _site, value);
            }
        }

        private string _batteryManufacturer = string.Empty;
        public string BatteryManufacturer
        {
            get => _batteryManufacturer;
            set
            {
                SetProperty(ref _batteryManufacturer, value);
            }
        }

        private string _batteryType = string.Empty;
        public string BatteryType
        {
            get => _batteryType;
            set
            {
                SetProperty(ref _batteryType, value);
            }
        }

        private Visibility _statusVisibility = Visibility.Collapsed;
        public Visibility StatusVisibility
        {
            get => _statusVisibility;
            set
            {
                SetProperty(ref _statusVisibility, value);
            }
        }

        private Visibility _cancelButtonVisibility = Visibility.Visible;
        public Visibility CancelButtonVisibility
        {
            get => _cancelButtonVisibility;
            set
            {
                SetProperty(ref _cancelButtonVisibility, value);
            }
        }

        private BatteryTransferStatusViewModel.ERequestStatus _statusEnum = BatteryTransferStatusViewModel.ERequestStatus.Waiting;
        public BatteryTransferStatusViewModel.ERequestStatus StatusEnum
        {
            get => _statusEnum;
            set
            {
                SetProperty(ref _statusEnum, value);
            }
        }

        private string _statusText = string.Empty;
        public string StatusText
        {
            get => _statusText;
            set
            {
                SetProperty(ref _statusText, value);
            }
        }

        private ObservableCollection<OutcomeSlotBinding> _outcomeWaitSlots_1F = new ObservableCollection<OutcomeSlotBinding>();
        public ObservableCollection<OutcomeSlotBinding> OutcomeWaitSlots_1F
        {
            get => _outcomeWaitSlots_1F;
            set
            {
                SetProperty(ref _outcomeWaitSlots_1F, value);
            }
        }

        private ObservableCollection<OutcomeSlotBinding> _outcomeWaitSlots_2F = new ObservableCollection<OutcomeSlotBinding>();
        public ObservableCollection<OutcomeSlotBinding> OutcomeWaitSlots_2F
        {
            get => _outcomeWaitSlots_2F;
            set
            {
                SetProperty(ref _outcomeWaitSlots_2F, value);
            }
        }

        public TransferInfoBinding(int row, int level, int column)
        {
            InitializeOutcomeSlotBinding(row, level, column);
        }

        private void InitializeOutcomeSlotBinding(int _row, int _level, int _column)
        {
            Row = _row;
            Level = _level;
            Column = _column;

            OutcomeWaitSlots_1F.Clear();
            OutcomeWaitSlots_2F.Clear();
            for (int row = 1; row <= RepoConstant.OutcomeWaitingRowCount; row++)
            {
                for (int level = 1; level <= RepoConstant.OutcomeWaitingLevelCount; level++)
                {
                    for (int column = 1; column <= RepoConstant.OutcomeWaitingColumnCount; column++)
                    {
                        var slot = new OutcomeSlotBinding();

                        if (row == _row && level == _level && column == _column)
                        {
                            slot.Background = ResColor.surface_action;
                        } else
                        {
                            slot.Background = ResColor.surface_secondary;
                        }

                        if (level == 1)
                        {
                            OutcomeWaitSlots_1F.Add(slot);
                        }
                        else if (level == 2)
                        {
                            OutcomeWaitSlots_2F.Add(slot);
                        }
                    }
                }
            }
        }
    }
}
