//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// AccountTypeMap
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
    public class AccountTypeMap : ClassMap<AccountType>
    {
        public AccountTypeMap()
        {
            Table("Account");
            Id(x => x.Id);
            Map(x => x.AccountName).Length(50);
            Map(x => x.AccountUrl).Length(255);
            Map(x => x.ApiKey).Length(200);
            Map(x => x.ApiSecret).Length(200);
            Map(x => x.ApiToken).Length(2000);
            Map(x => x.ApiTokenInfo).Length(2000);
            Map(x => x.IsDisplay);
            Map(x => x.Platform).Length(50);
            Map(x => x.Status);
            Map(x => x.Tax);
            Map(x => x.FromArea);
            Map(x => x.Description).Length(800);
            Map(x => x.Manager).Length(200);
            Map(x => x.Phone).Length(200);
            Map(x => x.Email).Length(200);
            Map(x => x.UserName);
            Map(x => x.AgreementPic);
            Map(x => x.DebitAccount);
            Map(x => x.WithdrawBy);
            Map(x => x.AlipayAccount);
            Map(x => x.RMBAccount);
            Map(x => x.USDAccount);
            Map(x => x.Amount1);
            Map(x => x.Amount2);
            Map(x => x.Amount3);
            Map(x => x.Amount4);
            Map(x => x.Amount5);
            Map(x => x.Remark);
            Map(x => x.Tixian1);
            Map(x => x.Tixian2);
            Map(x => x.TixianRate);
        }
    }
}
