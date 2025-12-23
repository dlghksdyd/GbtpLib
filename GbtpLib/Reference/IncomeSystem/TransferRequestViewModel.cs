using IncomeSystem.Design.Common;
using IncomeSystem.Repository;
using MExpress.Mex;
using MsSqlProcessor.MsSql;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace IncomeSystem.Design.TransferRequest
{
    public class TransferRequestViewModel : BindableBase
    {
        private static TransferRequestViewModel _instance = null;
        public static TransferRequestViewModel Instance
        {
            get => _instance ?? (_instance = new TransferRequestViewModel());
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

        public class RequestStatusBinding: BindableBase
        {
            private string _label = string.Empty;
            public string Label
            {
                get => _label;
                set => SetProperty(ref _label, value);
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

        private Visibility _incomeRejectRequestButtonVisibility = Visibility.Collapsed;
        public Visibility IncomeRejectRequestButtonVisibility
        {
            get => _incomeRejectRequestButtonVisibility;
            set => SetProperty(ref _incomeRejectRequestButtonVisibility, value);
        }

        private AllBatterySlotViewModel _allBatterySlotViewModel = null;

        public BatterySlotViewModel SelectedBatterySlot = null;

        private Thread _transferThread = null;
        private object _requestStatusLock = new object();

        public void Loaded()
        {
            if (_allBatterySlotViewModel == null)
            {
                _allBatterySlotViewModel = AllBatterySlotViewModel.Instance;
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
                            { "@IN_CMD_CD", EIF_CMD.AA2.ToString() },
                            { "@IN_DATA1", requestStatus.Label },
                            { "@IN_DATA2", requestStatus.Row },
                            { "@IN_DATA3", requestStatus.Column },
                            { "@IN_DATA4", requestStatus.Level },
                            { "@IN_REQ_SYS", RepoConstant.SystemCodeIncome }
                        };
                        bool result = MssqlBasic.ExecuteStoredProcedure("BRDS_ITF_CMD_DATA_SET", parameters);
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
                            { "@IN_CMD_CD", EIF_CMD.AA4.ToString() },
                            { "@IN_DATA1", requestStatus.Label },
                            { "@IN_DATA2", requestStatus.Row },
                            { "@IN_DATA3", requestStatus.Column },
                            { "@IN_DATA4", requestStatus.Level },
                            { "@IN_REQ_SYS", RepoConstant.SystemCodeIncome }
                        };
                        bool result = MssqlBasic.ExecuteStoredProcedure("BRDS_ITF_CMD_DATA_SET", parameters);
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
                        selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.CMD_CD), EIF_CMD.AA3.ToString());
                        selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.IF_FLAG), EIF_FLAG.C.ToString());
                        var itfCmdDataList = MssqlBasic.Select<ITF_CMD_DATA>(nameof(ITF_CMD_DATA), selectQueryBuilder);
                        if (itfCmdDataList.Count >= 1)
                        {
                            var itfCmdData = itfCmdDataList.First();
                            var updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(ITF_CMD_DATA));
                            updateQueryBuilder.AddSet(nameof(ITF_CMD_DATA.IF_FLAG), EIF_FLAG.Y);
                            updateQueryBuilder.AddWhere(nameof(ITF_CMD_DATA.CMD_CD), EIF_CMD.AA3.ToString());
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
                        selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.CMD_CD), EIF_CMD.AA5.ToString());
                        selectQueryBuilder.AddWhere(nameof(ITF_CMD_DATA), nameof(ITF_CMD_DATA.IF_FLAG), EIF_FLAG.C.ToString());
                        var itfCmdDataList = MssqlBasic.Select<ITF_CMD_DATA>(nameof(ITF_CMD_DATA), selectQueryBuilder);
                        if (itfCmdDataList.Count >= 1)
                        {
                            var itfCmdData = itfCmdDataList.First();
                            var updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(ITF_CMD_DATA));
                            updateQueryBuilder.AddSet(nameof(ITF_CMD_DATA.IF_FLAG), EIF_FLAG.Y);
                            updateQueryBuilder.AddWhere(nameof(ITF_CMD_DATA.CMD_CD), EIF_CMD.AA5.ToString());
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
                    RemoveRequestStatus(requestStatus);

                    // 인스턴스 업데이트
                    var batterySlot = _allBatterySlotViewModel.BatterySlots.ToList().Find(x =>
                        x.Row == requestStatus.Row &&
                        x.Column == requestStatus.Column &&
                        x.Level == requestStatus.Level);
                    _allBatterySlotViewModel.DeleteBattery(batterySlot);
                }
            }
        }

        public void RemoveRequestStatus(RequestStatusBinding requestStatus)
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

            var batterySlot = _allBatterySlotViewModel.BatterySlots.ToList().Find(x => x.BatteryInfo.Label == label);
            batterySlot.TransferRequest.EllipseVisibility = Visibility.Collapsed;
            batterySlot.TransferRequest.TransferOrder = 0;

            // Order 재설정
            for (int i = 0; i < RequestStatuses.Count; i++)
            {
                RequestStatuses[i].Order = i + 1;

                batterySlot = _allBatterySlotViewModel.BatterySlots.ToList().Find(x =>
                    x.BatteryInfo.Label == RequestStatuses[i].Label);
                batterySlot.TransferRequest.TransferOrder = RequestStatuses[i].Order;
            }

            if (SelectedBatterySlot != null)
            {
                if (SelectedBatterySlot.BatteryInfo.Label == label)
                {
                    IncomeRejectRequestButtonVisibility = Visibility.Visible;
                }
            }
        }

        public void SelectBatterySlot(BatterySlotViewModel batterySlot)
        {
            // 선택한 배터리가 입고 요청 중인지 확인
            int index = RequestStatuses.ToList().FindIndex(x => x.Label == batterySlot.BatteryInfo.Label);
            if (index >= 0) // 요청 중이면
            {
                // 입고 거부 버튼 비활성화
                IncomeRejectRequestButtonVisibility = Visibility.Collapsed;
            }
            else // 요청 중이지 않으면
            {
                // 입고 거부 버튼 활성화
                IncomeRejectRequestButtonVisibility = Visibility.Visible;
            }

            // 테두리 업데이트
            _allBatterySlotViewModel.BatterySlots.ToList().ForEach(x =>
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

            SelectedBatterySlot = batterySlot;
        }

        public void ClearBatterySelection()
        {
            // 테두리 업데이트
            _allBatterySlotViewModel.BatterySlots.ToList().ForEach(x =>
            {
                x.TransferRequest.BorderThickness = new Thickness(1);
                x.TransferRequest.BorderBrush = ResColor.border_primary;
            });

            // 버튼 업데이트
            IncomeRejectRequestButtonVisibility = Visibility.Collapsed;

            // 배터리 정보 업데이트
            BatteryInfo.BatteryInfoEmptyVisibility = Visibility.Visible;

            SelectedBatterySlot = null;
        }

        public void Reject()
        {
            AddTransferRequestStatus(SelectedBatterySlot, ETransferRequestType.Reject);
        }

        public void AddTransferRequestStatus(BatterySlotViewModel batterySlot, ETransferRequestType requestType)
        {
            int index = RequestStatuses.ToList().FindIndex(x => x.Label == batterySlot.BatteryInfo.Label);
            if (index < 0)
            {
                // RequestStatus 인스턴스 추가
                RequestStatusBinding requestStatus = new RequestStatusBinding();
                requestStatus.Label = batterySlot.BatteryInfo.Label;
                requestStatus.Row = batterySlot.Row;
                requestStatus.Column = batterySlot.Column;
                requestStatus.Level = batterySlot.Level;
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

        public void BulkRequest()
        {
            _allBatterySlotViewModel.BatterySlots.ToList().ForEach(x =>
            {
                if (x.InspectionInfo.InspectionResult == EInspectionResult.Pass)
                {
                    AddTransferRequestStatus(x, ETransferRequestType.Accept);
                }
                else if (x.InspectionInfo.InspectionResult == EInspectionResult.Fail)
                {
                    AddTransferRequestStatus(x, ETransferRequestType.Reject);
                }
            });
        }

        public void CancelRequest(RequestStatusBinding requestStatus)
        {
            RemoveRequestStatus(requestStatus);
        }
    }
}
