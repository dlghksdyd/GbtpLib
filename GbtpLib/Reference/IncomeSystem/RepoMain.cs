using IncomeSystem.Design;
using IncomeSystem.Design.Common;
using IncomeSystem.Design.Popup;
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

namespace IncomeSystem.Repository
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
        // 하차장 정보
        public static int UnloadingLevelCount { get; set; } = 3;

        // 입고 대기 장소 정보
        public static string IncomeWarehouseCode = "WH01";
        public static int IncomeWaitingRowCount { get; set; } = 1;
        public static int IncomeWaitingColumnCount { get; set; } = 6;
        public static int IncomeWaitingLevelCount { get; set; } = 2;

        // 시스템 코드
        public static string SystemCodeIncome { get; set; } = "IMS";
    }

    public static class RepoInfos
    {
        public class LabelCreationInfo
        {
            public string CAR_MAKE_CD { get; set; } = string.Empty;
            public string CAR_MAKE_NM { get; set; } = string.Empty;
            public string CAR_CD { get; set; } = string.Empty;
            public string CAR_NM { get; set; } = string.Empty;
            public string BTR_MAKE_CD { get; set; } = string.Empty;
            public string BTR_MAKE_NM { get; set; } = string.Empty;
            public string CAR_RELS_YEAR { get; set; } = string.Empty;
            public int BTR_TYPE_NO { get; set; } = 0;
            public string BTR_TYPE_SLT_CD { get; set; } = string.Empty;
            public string BTR_TYPE_NM { get; set; } = string.Empty;
        }

        public static List<LabelCreationInfo> LabelCreationInfos = new List<LabelCreationInfo>();

        public static void UpdateInfos(string WH_CD)
        {
            ThreadStart threadStart = () =>
            {
                UpdateLabelCreationInfos();
            };

            new PopupWaitViewModel()
            {
                Title = "메타 데이터 초기화",
            }.Open(threadStart);
        }

        public static void UpdateLabelCreationInfos()
        {
            var selectQueryBuilder = new MssqlSelectQueryBuilder();
            selectQueryBuilder.AddSelectColumns<MST_CAR_MAKE>(
                new string[]
                {
                    nameof(MST_CAR_MAKE.CAR_MAKE_CD),
                    nameof(MST_CAR_MAKE.CAR_MAKE_NM)
                });
            selectQueryBuilder.AddSelectColumns<MST_CAR>(
                new string[]
                {
                    nameof(MST_CAR.CAR_CD),
                    nameof(MST_CAR.CAR_NM)
                });
            selectQueryBuilder.AddSelectColumns<MST_BTR_TYPE>(
                new string[]
                {
                    nameof(MST_BTR_TYPE.BTR_MAKE_CD),
                    nameof(MST_BTR_TYPE.CAR_RELS_YEAR),
                    nameof(MST_BTR_TYPE.BTR_TYPE_NO),
                    nameof(MST_BTR_TYPE.BTR_TYPE_SLT_CD),
                    nameof(MST_BTR_TYPE.BTR_TYPE_NM),
                });
            selectQueryBuilder.AddSelectColumns<MST_BTR_MAKE>(
                new string[]
                {
                    nameof(MST_BTR_MAKE.BTR_MAKE_NM),
                });
            selectQueryBuilder.AddLeftJoin(
                nameof(MST_CAR_MAKE), nameof(MST_CAR_MAKE.CAR_MAKE_CD),
                nameof(MST_CAR), nameof(MST_CAR.CAR_MAKE_CD));
            selectQueryBuilder.AddLeftJoin(
                nameof(MST_CAR_MAKE), nameof(MST_CAR_MAKE.CAR_MAKE_CD),
                nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.CAR_MAKE_CD));
            selectQueryBuilder.AddLeftJoin(
                nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.BTR_MAKE_CD),
                nameof(MST_BTR_MAKE), nameof(MST_BTR_MAKE.BTR_MAKE_CD));
            selectQueryBuilder.AddWhere(nameof(MST_CAR_MAKE), nameof(MST_CAR_MAKE.USE_YN), EYN_FLAG.Y.ToString());
            selectQueryBuilder.AddWhere(nameof(MST_CAR), nameof(MST_CAR.USE_YN), EYN_FLAG.Y.ToString());
            selectQueryBuilder.AddWhere(nameof(MST_BTR_TYPE), nameof(MST_BTR_TYPE.USE_YN), EYN_FLAG.Y.ToString());
            selectQueryBuilder.AddWhere(nameof(MST_BTR_MAKE), nameof(MST_BTR_MAKE.USE_YN), EYN_FLAG.Y.ToString());

            LabelCreationInfos = MssqlBasic.Select<LabelCreationInfo>(nameof(MST_CAR_MAKE), selectQueryBuilder);
        }
    }
}
