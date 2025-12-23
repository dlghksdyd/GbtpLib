using IncomeSystem.Design.Common;
using IncomeSystem.Design.Popup;
using IncomeSystem.Repository;
using MExpress.Mex;
using MsSqlProcessor.MsSql;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace IncomeSystem.Design.LabelManagement
{
    public class LabelPrintViewModel : BindableBase
    {
        public class LabelInfoBinding : BindableBase
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

        private static LabelPrintViewModel _instance = null;
        public static LabelPrintViewModel Instance
        {
            get => _instance ?? (_instance = new LabelPrintViewModel());
        }

        private LabelInfoBinding _labelInfo = new LabelInfoBinding();
        public LabelInfoBinding LabelInfo
        {
            get => _labelInfo;
            set => SetProperty(ref _labelInfo, value);
        }

        private LabelManagementViewModel _labelManagementViewModel = null;
        private AllBatterySlotViewModel _allBatterySlotViewModel = null;

        private BatterySlotViewModel _selectedBatteryInfo = null;

        public void Loaded()
        {
            if (_labelManagementViewModel == null)
            {
                _labelManagementViewModel = LabelManagementViewModel.Instance;
            }

            if (_allBatterySlotViewModel == null)
            {
                _allBatterySlotViewModel = AllBatterySlotViewModel.Instance;
            }
        }

        public void Update(BatterySlotViewModel batteryInfo)
        {
            _selectedBatteryInfo = batteryInfo;

            LabelInfo.Label = batteryInfo.BatteryInfo.Label;
            LabelInfo.CarManufacturer = batteryInfo.BatteryInfo.CarManufacturer;
            LabelInfo.CarModel = batteryInfo.BatteryInfo.CarModel;
            LabelInfo.BatteryManufacturer = batteryInfo.BatteryInfo.BatteryManufacturer;
            LabelInfo.ReleaseYear = batteryInfo.BatteryInfo.ReleaseYear;
            LabelInfo.BatteryType = batteryInfo.BatteryInfo.BatteryType;
            LabelInfo.Site = batteryInfo.BatteryInfo.Site;
            LabelInfo.CollectionReason = batteryInfo.BatteryInfo.CollectionReason;
            LabelInfo.CollectionDate = batteryInfo.BatteryInfo.CollectionDate;
            LabelInfo.Mile = batteryInfo.BatteryInfo.Mile.ToString();
            LabelInfo.Memo = batteryInfo.BatteryInfo.Note;
        }

        public void DeleteLabel()
        {
            bool result = _allBatterySlotViewModel.DeleteLabel(_selectedBatteryInfo);

            if (result)
            {
                new PopupInfoViewModel()
                {
                    Title = "라벨 삭제",
                    Message = "라벨이 삭제 되었습니다.",
                    CancelButtonVisibility = System.Windows.Visibility.Collapsed
                }.Open();
            }
            else
            {
                new PopupWarningViewModel()
                {
                    Title = "라벨 삭제 오류",
                    Message = "라벨 삭제에 실패하였습니다.",
                    CancelButtonVisibility = System.Windows.Visibility.Collapsed
                }.Open();
            }
        }

        public void PrintLabel()
        {
            // TODO: 라벨 프린트 기능 추가해야함.

            new PopupInfoViewModel()
            {
                Title = "라벨 프린트",
                Message = "라벨이 프린트 되었습니다.",
                CancelButtonVisibility = System.Windows.Visibility.Collapsed
            }.Open();

            // DB에 있는 라벨 정보 업데이트
            var updateQueryBuilder = new MssqlUpdateQueryBuilder(nameof(MST_BTR));
            updateQueryBuilder.AddSet(nameof(MST_BTR.PRT_YN), EYN_FLAG.Y.ToString());
            updateQueryBuilder.AddWhere(nameof(MST_BTR.LBL_ID), _selectedBatteryInfo.BatteryInfo.Label);
            MssqlBasic.Update<MST_BTR>(updateQueryBuilder);

            // UI 업데이트
            _selectedBatteryInfo.BatteryInfo.PrintImage = new BitmapToImageSourceConverter().Convert(Images.IconImages.label_print_color, null, null, null) as BitmapSource;
            _labelManagementViewModel.ClearSlotSelection();
        }
    }
}
