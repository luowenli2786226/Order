using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App
{
    public enum OrderStatusEnum
    {
        待处理 = 0,
        已处理 = 1,
        待拣货 = 2,
        待包装 = 3,
        待发货 = 4,
        已发货 = 5,
        已完成 = 6,
        作废订单 = 7,
        退货订单 = 9
    }

    public enum ProductStatusEnum
    {
        销售中 = 0,
        热卖 = 1,
        滞销 = 2,
        清仓 = 3,
        停产 = 4,
        暂停销售 = 5,
        提价销售 = 6
    }

    public enum PlatformEnum
    {
        Aliexpress = 9999998,
        Ebay = 9999997,
        Amazon = 9999996,
        Gmarket = 9999994,
        DH = 9999993,
        WebSite = 9999992,
        Wish = 9999991,
        Bellabuy = 9999990,
        Lazada = 9999989,
        Cdiscount = 9999988,
        Rumall = 9999987
    }

    public enum ResourceCategoryEnum
    {
        User,
        Role,
        Department
    }
    public enum TargetCategoryEnum
    {
        Module,
        PermissionItem,
        Account
    }

    public enum PrintCategoryEnum
    {
        订单,
        多物品订单,
        商品
    }

    public enum RoleEnum
    {
        配货人员 = 0,
        配货检验人员 = 1,
        包装人员 = 2,
        清点人员 = 3,
        到货检验人员 = 4,
        邮件回复人员 = 5,
        邮件主管 = 6
    }

    public enum ProductAttributeEnum
    {
        粉末,
        液体,
        大电池,
        纽扣电池,
        仿牌,
        电子,
        磁铁,
        普货
    }

    public enum COnfigTempleCategory
    {
        单品单件 = 0,
        单品多件 = 1,
        多品多件 = 2
    }

    public enum Area
    {
        宁波,
        义乌
    }
    public enum OrderAction
    {
        分配发货仓库,
        匹配邮寄方式,
        需人工审核订单
    }
    public enum PickingListStateEnum
    {
        未打印,
        等待包装,
        正在包装,
        已包装
    }
    /// <summary>
    /// 海外仓审批字段
    /// </summary>
    public enum Shipmentapproval
    {
        未审核,
        审核中,
        审核通过,
        审核拒绝,
        确认通过I,
        确认拒绝I,
        确认通过II,
        确认拒绝II
    }
}