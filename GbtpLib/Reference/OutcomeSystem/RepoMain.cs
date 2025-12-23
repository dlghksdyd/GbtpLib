using OutcomeSystem.Design;
using OutcomeSystem.Design.Popup;
using MExpress.Mex;
using MsSqlProcessor.MsSql;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace OutcomeSystem.Repository
{
    /// <summary>
    /// 로그인 정보
    /// </summary>
    public static class RepoUser
    {
        public static string UserId { get; set; } = string.Empty;
        public static string Password { get; set; } = string.Empty;
        public static bool IsAdmin { get; set; } = false;
    }

    /// <summary>
    /// 상수 값들
    /// </summary>
    public static class RepoConstant
    {
        // 상차장 정보
        public static int LoadingRowCount { get; set; } = 1;
        public static int LoadingColumnCount { get; set; } = 1;
        public static int LoadingLevelCount { get; set; } = 3;

        // 출고 대기 장소 정보
        public static string OutcomeWarehouseCode = "WH02";
        public static int OutcomeWaitingRowCount = 1;
        public static int OutcomeWaitingColumnCount = 5;
        public static int OutcomeWaitingLevelCount = 2;

        // 등급 분류 적재 창고 정보
        public static string GradeWarehouseCode = "WH04";

        // 시스템 코드
        public static string SystemCodeOutcome { get; set; } = "SMS";

        // 공통코드 그룹
        public static string CommonCodeGroup_Grade = "INV002";
    }
}
