using MExpress.Mex;
using MsSqlProcessor.MsSql;
using OutcomeSystem.Design.Common;
using OutcomeSystem.Design.DefectBattery;
using OutcomeSystem.Design.Popup;
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

namespace OutcomeSystem.Design.OutcomeWaitBattery
{
    public class OutcomeWaitBatteryViewModel : BindableBase
    {
        private static OutcomeWaitBatteryViewModel _instance;
        public static OutcomeWaitBatteryViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new OutcomeWaitBatteryViewModel();
                }
                return _instance;
            }
        }

        public enum ETransferRequestType
        {
            Accept,
            Reject,
        }

        public enum ERequestStatus
        {
            Waiting,
            InProgress,
            Finished,
        }

        public enum ERemoveType
        {
            Cancel,
            Finish,
        }

        public class RequestStatusBinding : BindableBase
        {
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

            private int _outcomeWaitRow = 0;
            public int OutcomeWaitRow
            {
                get => _outcomeWaitRow;
                set => SetProperty(ref _outcomeWaitRow, value);
            }

            private int _outcomeWaitColumn = 0;
            public int OutcomeWaitColumn
            {
                get => _outcomeWaitColumn;
                set => SetProperty(ref _outcomeWaitColumn, value);
            }

            private int _outcomeWaitLevel = 0;
            public int OutcomeWaitLevel
            {
                get => _outcomeWaitLevel;
                set => SetProperty(ref _outcomeWaitLevel, value);
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

            private Visibility _acceptVisibility = Visibility.Collapsed;
            public Visibility AcceptVisibility
            {
                get => _acceptVisibility;
                set => SetProperty(ref _acceptVisibility, value);
            }

            private Visibility _rejectVisibility = Visibility.Collapsed;
            public Visibility RejectVisibility
            {
                get => _rejectVisibility;
                set => SetProperty(ref _rejectVisibility, value);
            }

            private string _statusText = string.Empty;
            public string StatusText
            {
                get => _statusText;
                set => SetProperty(ref _statusText, value);
            }

            private ERequestStatus _status = ERequestStatus.Waiting;
            public ERequestStatus StatusEnum
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


        public class BatteryInfoBinding : BindableBase
        {
            private Visibility _batteryInfoEmptyVisibility = Visibility.Visible;
            public Visibility BatteryInfoEmptyVisibility
            {
                get => _batteryInfoEmptyVisibility;
                set => SetProperty(ref _batteryInfoEmptyVisibility, value);
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

            private string _site = string.Empty;
            public string Site
            {
                get => _site;
                set => SetProperty(ref _site, value);
            }

            private string _grade = string.Empty;
            public string Grade
            {
                get => _grade;
                set => SetProperty(ref _grade, value);
            }

            private string _collectionReason = string.Empty;
            public string CollectionReason
            {
                get => _collectionReason;
                set => SetProperty(ref _collectionReason, value);
            }
        }

        private ObservableCollection<RequestStatusBinding> _requestStatuses = new ObservableCollection<RequestStatusBinding>();
        public ObservableCollection<RequestStatusBinding> RequestStatuses
        {
            get => _requestStatuses;
            set => SetProperty(ref _requestStatuses, value);
        }

        private BatteryInfoBinding _batteryInfo = new BatteryInfoBinding();
        public BatteryInfoBinding BatteryInfo
        {
            get => _batteryInfo;
            set => SetProperty(ref _batteryInfo, value);
        }

        private Visibility _outcomeRejectRequestButtonVisibility = Visibility.Collapsed;
        public Visibility OutcomeRejectRequestButtonVisibility
        {
            get => _outcomeRejectRequestButtonVisibility;
            set => SetProperty(ref _outcomeRejectRequestButtonVisibility, value);
        }

        private Visibility _outcomeAcceptRequestButtonVisibility = Visibility.Collapsed;
        public Visibility OutcomeAcceptRequestButtonVisibility
        {
            get => _outcomeAcceptRequestButtonVisibility;
            set => SetProperty(ref _outcomeAcceptRequestButtonVisibility, value);
        }

        private AllBatterySlotViewModel _allBatterySlotViewModel = null;
        private DefectBatteryViewModel _defectBatteryViewModel = null;

        public OutcomeWaitBatterySlotViewModel SelectedBatterySlot = null;

        private Thread _transferThread = null;
        private object _requestStatusLock = new object();

        public void Loaded()
        {
            if (_allBatterySlotViewModel == null)
            {
                _allBatterySlotViewModel = AllBatterySlotViewModel.Instance;
            }
            if (_defectBatteryViewModel == null)
            {
                _defectBatteryViewModel = DefectBatteryViewModel.Instance;
            }

            if (_transferThread == null)
            {
                _transferThread = new Thread(TransferThread);
                _transferThread.IsBackground = true;
                _transferThread.Start();
            }
        }

        public void TransferThread()
        {
            while (true)
            {
                Thread.Sleep(1000);

                if (RequestStatuses.Count == 0) continue;

                var requestStatus = RequestStatuses.First();

                if (requestStatus.StatusEnum == ERequestStatus.Waiting)
                {
                    if (requestStatus.AcceptVisibility == Visibility.Visible)
                    {
                        var parameters = new Dictionary<string, object>
                        {
                            { "@IN_CMD_CD", EIF_CMD.EE3.ToString() },
                            { "@IN_DATA1", requestStatus.Label },
                            { "@IN_DATA2", requestStatus.LoadingRow },
                            { "@IN_DATA3", requestStatus.LoadingColumn },
                            { "@IN_DATA4", requestStatus.LoadingLevel },
                            { "@IN_REQ_SYS", RepoConstant.SystemCodeOutcome }
                        };
                        bool result = MssqlBasic.ExecuteStoredProcedure("BRDS_ITF_CMD_DATA_SET", parameters);
                        //bool result = true;
                        if (result)
                        {
                            requestStatus.StatusText = "이송 중";
                            requestStatus.StatusEnum = ERequestStatus.InProgress;
                            requestStatus.CancelImageVisibility = Visibility.Hidden;
                        }
                    }
                    else if (requestStatus.RejectVisibility == Visibility.Visible)
                    {
                        var parameters = new Dictionary<string, object>
                        {
                            { "@IN_CMD_CD", EIF_CMD.EE5.ToString() },
                            { "@IN_DATA1", requestStatus.Label },
                            { "@IN_DATA2", requestStatus.OutcomeWaitRow },
                            { "@IN_DATA3", requestStatus.OutcomeWaitColumn },
                            { "@IN_DATA4", requestStatus.OutcomeWaitLevel },
                            { "@IN_REQ_SYS", RepoConstant.SystemCodeOutcome }
                        };
                        bool result = MssqlBasic.ExecuteStoredProcedure("BRDS_ITF_CMD_DATA_SET", parameters);
                        //bool result = true;
                        if (result)
                        {
                            requestStatus.StatusText = "이송 중";
                            requestStatus.StatusEnum = ERequestStatus.InProgress;
                            requestStatus.CancelImageVisibility = Visibility.Hidden;
                        }
                    }
                }
                else if (requestStatus.StatusEnum == ERequestStatus.InProgress)
                {
                    if (requestStatus.AcceptVisibility == Visibility.Visible)
                    {
                        var selectQueryBuilder = new MssqlSelectQueryBuilder();
                        selectQueryBuilder.AddSelectColumns<ITF_CMD_DATA>(new string[]
                        {
                            nameof(ITF_CMD_DATA.CMD_CD),
                        });
                        selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.DATA1), requestStatus.Label);
                        selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.CMD_CD), EIF_CMD.EE4.ToString());
                        selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.IF_FLAG), EIF_FLAG.C.ToString());
                        var itfCmdDataList = MssqlBasic.Select<ITF_CMD_DATA>(nameof(ITF_CMD_DATA), selectQueryBuilder);
                        if (itfCmdDataList.Count >= 1)
                        {
                            var itfCmdData = itfCmdDataList.First();
                            var updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(ITF_CMD_DATA));
                            updateQueryBuilder.AddSet(nameof(ITF_CMD_DATA.IF_FLAG), EIF_FLAG.Y);
                            updateQueryBuilder.AddWhere(nameof(ITF_CMD_DATA.CMD_CD), EIF_CMD.EE4.ToString());
                            updateQueryBuilder.AddWhere(nameof(ITF_CMD_DATA.DATA1), requestStatus.Label);
                            int result = MssqlBasic.Update<ITF_CMD_DATA>(updateQueryBuilder);
                            if (result >= 1)
                            {
                                requestStatus.StatusText = "이송 완료";
                                requestStatus.StatusEnum = ERequestStatus.Finished;
                            }
                        }
                    }
                    else if (requestStatus.RejectVisibility == Visibility.Visible)
                    {
                        var selectQueryBuilder = new MssqlSelectQueryBuilder();
                        selectQueryBuilder.AddSelectColumns<ITF_CMD_DATA>(new string[]
                        {
                            nameof(ITF_CMD_DATA.CMD_CD),
                        });
                        selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.DATA1), requestStatus.Label);
                        selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.CMD_CD), EIF_CMD.EE6.ToString());
                        selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.IF_FLAG), EIF_FLAG.C.ToString());
                        var itfCmdDataList = MssqlBasic.Select<ITF_CMD_DATA>(nameof(ITF_CMD_DATA), selectQueryBuilder);
                        if (itfCmdDataList.Count >= 1)
                        {
                            var itfCmdData = itfCmdDataList.First();
                            var updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(ITF_CMD_DATA));
                            updateQueryBuilder.AddSet(nameof(ITF_CMD_DATA.IF_FLAG), EIF_FLAG.Y);
                            updateQueryBuilder.AddWhere(nameof(ITF_CMD_DATA.CMD_CD), EIF_CMD.EE6.ToString());
                            updateQueryBuilder.AddWhere(nameof(ITF_CMD_DATA.DATA1), requestStatus.Label);
                            int result = MssqlBasic.Update<ITF_CMD_DATA>(updateQueryBuilder);
                            if (result >= 1)
                            {
                                requestStatus.StatusText = "이송 완료";
                                requestStatus.StatusEnum = ERequestStatus.Finished;
                            }
                        }
                    }
                }
                else if (requestStatus.StatusEnum == ERequestStatus.Finished)
                {
                    // UI 업데이트
                    RemoveRequestStatus(requestStatus, ERemoveType.Finish);

                    // 인스턴스 업데이트
                    var batterySlot = _allBatterySlotViewModel.OutcomeWaitBatterySlots.ToList().Find(x =>
                        x.Row == requestStatus.OutcomeWaitRow &&
                        x.Column == requestStatus.OutcomeWaitColumn &&
                        x.Level == requestStatus.OutcomeWaitLevel);
                    _allBatterySlotViewModel.DeleteOutcomeWaitBattery(batterySlot);

                    if (requestStatus.AcceptVisibility == Visibility.Visible)
                    {
                        _allBatterySlotViewModel.AddLoadingBattery(requestStatus);
                    }
                }
            }
        }

        public void RemoveRequestStatus(RequestStatusBinding requestStatus, ERemoveType removeType)
        {
            string label = requestStatus.Label;

            // RequestStatus 제거
            lock (_requestStatusLock)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    int index = RequestStatuses.ToList().FindIndex(x => x.Label == requestStatus.Label);
                    if (index >= 0)
                    {
                        RequestStatuses.RemoveAt(index);
                    }
                });
            }

            var batterySlot = _allBatterySlotViewModel.OutcomeWaitBatterySlots.ToList().Find(x => x.BatteryInfo.Label == label);
            batterySlot.TransferRequest.EllipseVisibility = Visibility.Collapsed;
            batterySlot.TransferRequest.TransferOrder = 0;

            // Order 재설정
            for (int i = 0; i < RequestStatuses.Count; i++)
            {
                RequestStatuses[i].Order = i + 1;

                batterySlot = _allBatterySlotViewModel.OutcomeWaitBatterySlots.ToList().Find(x =>
                    x.BatteryInfo.Label == RequestStatuses[i].Label);
                batterySlot.TransferRequest.TransferOrder = RequestStatuses[i].Order;
            }

            if (SelectedBatterySlot != null)
            {
                if (SelectedBatterySlot.BatteryInfo.Label == label)
                {
                    if (removeType == ERemoveType.Cancel)
                    {
                        OutcomeRejectRequestButtonVisibility = Visibility.Visible;
                        OutcomeAcceptRequestButtonVisibility = Visibility.Visible;
                    }
                    else if (removeType == ERemoveType.Finish)
                    {
                        ClearBatterySelection();
                    }
                }
            }
        }

        public void SelectBatterySlot(OutcomeWaitBatterySlotViewModel batterySlot)
        {
            // 선택한 배터리가 출고 요청 중인지 확인
            int index = RequestStatuses.ToList().FindIndex(x => x.Label == batterySlot.BatteryInfo.Label);
            if (index >= 0) // 요청 중이면
            {
                // 출고 버튼 비활성화
                OutcomeRejectRequestButtonVisibility = Visibility.Collapsed;
                OutcomeAcceptRequestButtonVisibility = Visibility.Collapsed;
            }
            else // 요청 중이지 않으면
            {
                // 출고 버튼 활성화
                OutcomeRejectRequestButtonVisibility = Visibility.Visible;
                OutcomeAcceptRequestButtonVisibility = Visibility.Visible;
            }

            // 테두리 업데이트
            _allBatterySlotViewModel.OutcomeWaitBatterySlots.ToList().ForEach(x =>
            {
                x.TransferRequest.BorderThickness = new Thickness(1);
                x.TransferRequest.BorderBrush = ResColor.border_primary;
            });
            batterySlot.TransferRequest.BorderThickness = new Thickness(2);
            batterySlot.TransferRequest.BorderBrush = ResColor.border_action;

            // 배터리 정보 업데이트
            BatteryInfo.BatteryInfoEmptyVisibility = Visibility.Collapsed;
            BatteryInfo.Label = batterySlot.BatteryInfo.Label;
            BatteryInfo.CarManufacturer = batterySlot.BatteryInfo.CarManufacturer;
            BatteryInfo.CarModel = batterySlot.BatteryInfo.CarModel;
            BatteryInfo.BatteryManufacturer = batterySlot.BatteryInfo.BatteryManufacturer;
            BatteryInfo.ReleaseYear = batterySlot.BatteryInfo.ReleaseYear;
            BatteryInfo.PackOrModule = batterySlot.BatteryInfo.PackOrModule;
            BatteryInfo.BatteryType = batterySlot.BatteryInfo.BatteryType;
            BatteryInfo.Site = batterySlot.BatteryInfo.Site;
            BatteryInfo.CollectionReason = batterySlot.BatteryInfo.CollectionReason;
            BatteryInfo.Grade = batterySlot.BatteryInfo.Grade;

            SelectedBatterySlot = batterySlot;
        }

        public void ClearBatterySelection()
        {
            // 테두리 업데이트
            _allBatterySlotViewModel.OutcomeWaitBatterySlots.ToList().ForEach(x =>
            {
                x.TransferRequest.BorderThickness = new Thickness(1);
                x.TransferRequest.BorderBrush = ResColor.border_primary;
            });

            // 버튼 업데이트
            OutcomeRejectRequestButtonVisibility = Visibility.Collapsed;
            OutcomeAcceptRequestButtonVisibility = Visibility.Collapsed;

            // 배터리 정보 업데이트
            BatteryInfo.BatteryInfoEmptyVisibility = Visibility.Visible;

            SelectedBatterySlot = null;
        }

        public void Accept()
        {
            // 선택된 배터리가 없는 경우
            if (SelectedBatterySlot == null)
            {
                new PopupWarningViewModel()
                {
                    Title = "출고 요청 실패",
                    Message = "선택된 배터리가 없습니다.",
                    CancelButtonVisibility = Visibility.Collapsed
                }.Open();
                return;
            }

            // 불량 창고에서 상차장으로 전송중인 배터리가 있을 경우
            if (_defectBatteryViewModel.TransferBattery.Label != string.Empty)
            {
                new PopupWarningViewModel()
                {
                    Title = "출고 요청 실패",
                    Message = "불량 창고에서 상차장으로 이송중인 배터리가 있습니다.",
                    CancelButtonVisibility = Visibility.Collapsed
                }.Open();
                return;
            }

            foreach (var oneLoadingBatterySlot in _allBatterySlotViewModel.LoadingBatterySlots)
            {
                // 상차장에 불량 등급 배터리가 있는 경우
                if (oneLoadingBatterySlot.BatteryInfo.Grade == "F")
                {
                    new PopupWarningViewModel()
                    {
                        Title = "출고 요청 실패",
                        Message = "상차장에 불량등급(등급 F)인 배터리가 있습니다. \n등급이 F인 배터리는 2단이상 적재할 수 없습니다.",
                        CancelButtonVisibility = Visibility.Collapsed
                    }.Open();
                    return;
                }
            }

            bool result = new PopupBarcodeScanViewModel()
            {
                Label = SelectedBatterySlot.BatteryInfo.Label,
            }.Open();
            if (!result) return;

            var popupOutgoingRequestViewModel = new PopupOutgoingRequestViewModel()
            {
                CarManufacturer = SelectedBatterySlot.BatteryInfo.CarManufacturer,
                CarModel = SelectedBatterySlot.BatteryInfo.CarModel,
                ReleaseYear = SelectedBatterySlot.BatteryInfo.ReleaseYear,
                BatteryManufacturer = SelectedBatterySlot.BatteryInfo.BatteryManufacturer,
                BatteryType = SelectedBatterySlot.BatteryInfo.BatteryType,
                Grade = SelectedBatterySlot.BatteryInfo.Grade,
            };
            result = popupOutgoingRequestViewModel.Open();
            if (!result) return;

            SelectedBatterySlot.LoadingPosition.Row = 1;
            SelectedBatterySlot.LoadingPosition.Column = 1;
            SelectedBatterySlot.LoadingPosition.Level = popupOutgoingRequestViewModel.SelectedLevel;
            AddTransferRequestStatus(SelectedBatterySlot, ETransferRequestType.Accept);
        }

        public void Reject()
        {
            if (SelectedBatterySlot == null) return;

            AddTransferRequestStatus(SelectedBatterySlot, ETransferRequestType.Reject);
        }

        public void AddTransferRequestStatus(OutcomeWaitBatterySlotViewModel batterySlot, ETransferRequestType requestType)
        {
            int index = RequestStatuses.ToList().FindIndex(x => x.Label == batterySlot.BatteryInfo.Label);
            if (index < 0)
            {
                // RequestStatus 인스턴스 추가
                RequestStatusBinding requestStatus = new RequestStatusBinding();
                requestStatus.Label = batterySlot.BatteryInfo.Label;
                requestStatus.CarManufacturer = batterySlot.BatteryInfo.CarManufacturer;
                requestStatus.CarModel = batterySlot.BatteryInfo.CarModel;
                requestStatus.BatteryManufacturer = batterySlot.BatteryInfo.BatteryManufacturer;
                requestStatus.ReleaseYear = batterySlot.BatteryInfo.ReleaseYear;
                requestStatus.PackOrModule = batterySlot.BatteryInfo.PackOrModule;
                requestStatus.BatteryType = batterySlot.BatteryInfo.BatteryType;
                requestStatus.Site = batterySlot.BatteryInfo.Site;
                requestStatus.CollectionReason = batterySlot.BatteryInfo.CollectionReason;
                requestStatus.Grade = batterySlot.BatteryInfo.Grade;
                requestStatus.OutcomeWaitRow = batterySlot.Row;
                requestStatus.OutcomeWaitColumn = batterySlot.Column;
                requestStatus.OutcomeWaitLevel = batterySlot.Level;
                requestStatus.LoadingRow = batterySlot.LoadingPosition.Row;
                requestStatus.LoadingColumn = batterySlot.LoadingPosition.Column;
                requestStatus.LoadingLevel = batterySlot.LoadingPosition.Level;
                requestStatus.Order = RequestStatuses.Count + 1;
                requestStatus.Position = $"{batterySlot.Level}F-{batterySlot.Column}";
                if (requestType == ETransferRequestType.Accept)
                {
                    requestStatus.AcceptVisibility = Visibility.Visible;
                    requestStatus.RejectVisibility = Visibility.Collapsed;
                }
                else if (requestType == ETransferRequestType.Reject)
                {
                    requestStatus.AcceptVisibility = Visibility.Collapsed;
                    requestStatus.RejectVisibility = Visibility.Visible;
                }
                requestStatus.StatusText = "대기 중";
                requestStatus.StatusEnum = ERequestStatus.Waiting;
                requestStatus.CancelImageVisibility = Visibility.Visible;

                lock (_requestStatusLock)
                {
                    RequestStatuses.Add(requestStatus);
                }

                // 배터리 슬롯 UI 업데이트
                batterySlot.TransferRequest.EllipseVisibility = Visibility.Visible;
                batterySlot.TransferRequest.TransferOrder = requestStatus.Order;

                ClearBatterySelection();
            }
        }

        public void CancelRequest(RequestStatusBinding requestStatus)
        {
            // 현재 요청 보다 뒤에 있는 요청들까지 모두 삭제
            int index = RequestStatuses.ToList().FindIndex(x => x.Label == requestStatus.Label);
            while (index < RequestStatuses.Count)
            {
                RemoveRequestStatus(RequestStatuses[index], ERemoveType.Cancel);
            }
        }
    }
}
