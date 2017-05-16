//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// AccountType
    /// 平台账户表
    /// 
    /// 修改纪录
    /// 
    ///  版本：1.0  创建主键。
    /// 
    /// 版本：1.0
    /// 
    /// <author>
    /// <name></name>
    /// <date></date>
    /// </author>
    /// </summary>
    public class AccountType
    {
        /// <summary>
        /// 主键
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 显示
        /// </summary>
        public virtual int IsDisplay { get; set; }

        /// <summary>
        /// 账户名称
        /// </summary>
        public virtual String AccountName { get; set; }

        /// <summary>
        /// 平台网址
        /// </summary>
        public virtual String AccountUrl { get; set; }

        /// <summary>
        /// APIKey
        /// </summary>
        public virtual String ApiKey { get; set; }

        /// <summary>
        /// API密钥
        /// </summary>
        public virtual String ApiSecret { get; set; }

        /// <summary>
        /// API会话
        /// </summary>
        public virtual String ApiToken { get; set; }


        /// <summary>
        /// API会话
        /// </summary>
        public virtual String ApiTokenInfo { get; set; }

        /// <summary>
        /// 平台
        /// </summary>
        public virtual String Platform { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public virtual int Status { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public virtual String Description { get; set; }

        /// <summary>
        /// 负责人
        /// </summary>
        public virtual String Manager { get; set; }

        /// <summary>
        /// 负责人电话
        /// </summary>
        public virtual String Phone { get; set; }

        /// <summary>
        /// 负责人邮箱
        /// </summary>
        public virtual String Email { get; set; }

        /// <summary>
        /// 负责人邮箱
        /// </summary>
        public virtual String FromArea { get; set; }

        /// <summary>
        /// 负责人邮箱
        /// </summary>
        public virtual String USDAccount { get; set; }

        /// <summary>
        /// 负责人邮箱
        /// </summary>
        public virtual String RMBAccount { get; set; }

        /// <summary>
        /// 负责人邮箱
        /// </summary>
        public virtual String AlipayAccount { get; set; }

        /// <summary>
        /// 负责人邮箱
        /// </summary>
        public virtual String WithdrawBy { get; set; }


        /// <summary>
        /// 负责人邮箱
        /// </summary>
        public virtual String DebitAccount { get; set; }

        /// <summary>
        /// 负责人邮箱
        /// </summary>
        public virtual String UserName { get; set; }

        /// <summary>
        /// 负责人邮箱
        /// </summary>
        public virtual String AgreementPic { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public virtual String Icon { get; set; }

        /// <summary>
        /// 总销售额
        /// </summary>
        public virtual decimal Amount1 { get; set; }


        /// <summary>
        /// 费率
        /// </summary>
        public virtual decimal Tax { get; set; }
        /// <summary>
        /// 总提现美金
        /// </summary>
        public virtual decimal Amount2 { get; set; }

        /// <summary>
        /// 总提现人民币
        /// </summary>
        public virtual decimal Amount5 { get; set; }

        /// <summary>
        /// 余额美金
        /// </summary>
        public virtual decimal Amount3 { get; set; }

        /// <summary>
        /// 余额人民币
        /// </summary>
        public virtual decimal Amount4 { get; set; }

        //备注
        public virtual String Remark { get; set; }
        /// <summary>
        /// 提现方式1
        /// </summary>
        public virtual string Tixian1 { get; set; }
        /// <summary>
        /// 提现方式2
        /// </summary>
        public virtual string Tixian2 { get; set; }
        /// <summary>
        /// 提现手续费
        /// </summary>
        public virtual string TixianRate { get; set; }
    }
}
