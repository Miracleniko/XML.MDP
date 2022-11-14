﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using XML.Core.Data;
using XML.Core.Log;
using XML.Core.Model;
using XML.Core.Stub;
using XML.Core;
using XML.XCode.Configuration;
using XML.XCode.DataAccessLayer;

namespace XML.XCode.Membership;

/// <summary>用户。用户帐号信息</summary>
[Serializable]
[DataObject]
[Description("用户。用户帐号信息")]
[BindIndex("IU_User_Name", true, "Name")]
[BindIndex("IX_User_Mail", false, "Mail")]
[BindIndex("IX_User_Mobile", false, "Mobile")]
[BindIndex("IX_User_Code", false, "Code")]
[BindIndex("IX_User_RoleID", false, "RoleID")]
[BindIndex("IX_User_UpdateTime", false, "UpdateTime")]
[BindTable("User", Description = "用户。用户帐号信息", ConnName = "Membership", DbType = DatabaseType.None)]
public partial class User
{
    #region 属性
    private Int32 _ID;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, true, false, 0)]
    [BindColumn("ID", "编号", "")]
    public Int32 ID { get => _ID; set { if (OnPropertyChanging("ID", value)) { _ID = value; OnPropertyChanged("ID"); } } }

    private String _Name;
    /// <summary>名称。登录用户名</summary>
    [DisplayName("名称")]
    [Description("名称。登录用户名")]
    [DataObjectField(false, false, false, 50)]
    [BindColumn("Name", "名称。登录用户名", "", Master = true)]
    public String Name { get => _Name; set { if (OnPropertyChanging("Name", value)) { _Name = value; OnPropertyChanged("Name"); } } }

    private String _Password;
    /// <summary>密码</summary>
    [DisplayName("密码")]
    [Description("密码")]
    [DataObjectField(false, false, true, 200)]
    [BindColumn("Password", "密码", "")]
    public String Password { get => _Password; set { if (OnPropertyChanging("Password", value)) { _Password = value; OnPropertyChanged("Password"); } } }

    private String _DisplayName;
    /// <summary>昵称</summary>
    [DisplayName("昵称")]
    [Description("昵称")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("DisplayName", "昵称", "")]
    public String DisplayName { get => _DisplayName; set { if (OnPropertyChanging("DisplayName", value)) { _DisplayName = value; OnPropertyChanged("DisplayName"); } } }

    private XCode.Membership.SexKinds _Sex;
    /// <summary>性别。未知、男、女</summary>
    [DisplayName("性别")]
    [Description("性别。未知、男、女")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Sex", "性别。未知、男、女", "")]
    public XCode.Membership.SexKinds Sex { get => _Sex; set { if (OnPropertyChanging("Sex", value)) { _Sex = value; OnPropertyChanged("Sex"); } } }

    private String _Mail;
    /// <summary>邮件</summary>
    [DisplayName("邮件")]
    [Description("邮件")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Mail", "邮件", "", ItemType = "mail")]
    public String Mail { get => _Mail; set { if (OnPropertyChanging("Mail", value)) { _Mail = value; OnPropertyChanged("Mail"); } } }

    private String _Mobile;
    /// <summary>手机</summary>
    [DisplayName("手机")]
    [Description("手机")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Mobile", "手机", "", ItemType = "mobile")]
    public String Mobile { get => _Mobile; set { if (OnPropertyChanging("Mobile", value)) { _Mobile = value; OnPropertyChanged("Mobile"); } } }

    private String _Code;
    /// <summary>代码。身份证、员工编号等</summary>
    [DisplayName("代码")]
    [Description("代码。身份证、员工编号等")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Code", "代码。身份证、员工编号等", "")]
    public String Code { get => _Code; set { if (OnPropertyChanging("Code", value)) { _Code = value; OnPropertyChanged("Code"); } } }

    private Int32 _AreaId;
    /// <summary>地区。省市区</summary>
    [DisplayName("地区")]
    [Description("地区。省市区")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("AreaId", "地区。省市区", "")]
    public Int32 AreaId { get => _AreaId; set { if (OnPropertyChanging("AreaId", value)) { _AreaId = value; OnPropertyChanged("AreaId"); } } }

    private String _Avatar;
    /// <summary>头像</summary>
    [DisplayName("头像")]
    [Description("头像")]
    [DataObjectField(false, false, true, 200)]
    [BindColumn("Avatar", "头像", "", ItemType = "image")]
    public String Avatar { get => _Avatar; set { if (OnPropertyChanging("Avatar", value)) { _Avatar = value; OnPropertyChanged("Avatar"); } } }

    private Int32 _RoleID;
    /// <summary>角色。主要角色</summary>
    [Category("登录信息")]
    [DisplayName("角色")]
    [Description("角色。主要角色")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("RoleID", "角色。主要角色", "", DefaultValue = "3")]
    public Int32 RoleID { get => _RoleID; set { if (OnPropertyChanging("RoleID", value)) { _RoleID = value; OnPropertyChanged("RoleID"); } } }

    private String _RoleIds;
    /// <summary>角色组。次要角色集合</summary>
    [Category("登录信息")]
    [DisplayName("角色组")]
    [Description("角色组。次要角色集合")]
    [DataObjectField(false, false, true, 200)]
    [BindColumn("RoleIds", "角色组。次要角色集合", "")]
    public String RoleIds { get => _RoleIds; set { if (OnPropertyChanging("RoleIds", value)) { _RoleIds = value; OnPropertyChanged("RoleIds"); } } }

    private Int32 _DepartmentID;
    /// <summary>部门。组织机构</summary>
    [Category("登录信息")]
    [DisplayName("部门")]
    [Description("部门。组织机构")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("DepartmentID", "部门。组织机构", "")]
    public Int32 DepartmentID { get => _DepartmentID; set { if (OnPropertyChanging("DepartmentID", value)) { _DepartmentID = value; OnPropertyChanged("DepartmentID"); } } }

    private Boolean _Online;
    /// <summary>在线</summary>
    [Category("登录信息")]
    [DisplayName("在线")]
    [Description("在线")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Online", "在线", "")]
    public Boolean Online { get => _Online; set { if (OnPropertyChanging("Online", value)) { _Online = value; OnPropertyChanged("Online"); } } }

    private Boolean _Enable;
    /// <summary>启用</summary>
    [Category("登录信息")]
    [DisplayName("启用")]
    [Description("启用")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Enable", "启用", "")]
    public Boolean Enable { get => _Enable; set { if (OnPropertyChanging("Enable", value)) { _Enable = value; OnPropertyChanged("Enable"); } } }

    private Int32 _Age;
    /// <summary>年龄。周岁</summary>
    [DisplayName("年龄")]
    [Description("年龄。周岁")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Age", "年龄。周岁", "")]
    public Int32 Age { get => _Age; set { if (OnPropertyChanging("Age", value)) { _Age = value; OnPropertyChanged("Age"); } } }

    private DateTime _Birthday;
    /// <summary>生日。公历年月日</summary>
    [DisplayName("生日")]
    [Description("生日。公历年月日")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("Birthday", "生日。公历年月日", "")]
    public DateTime Birthday { get => _Birthday; set { if (OnPropertyChanging("Birthday", value)) { _Birthday = value; OnPropertyChanged("Birthday"); } } }

    private Int32 _Logins;
    /// <summary>登录次数</summary>
    [Category("登录信息")]
    [DisplayName("登录次数")]
    [Description("登录次数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Logins", "登录次数", "")]
    public Int32 Logins { get => _Logins; set { if (OnPropertyChanging("Logins", value)) { _Logins = value; OnPropertyChanged("Logins"); } } }

    private DateTime _LastLogin;
    /// <summary>最后登录</summary>
    [Category("登录信息")]
    [DisplayName("最后登录")]
    [Description("最后登录")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("LastLogin", "最后登录", "")]
    public DateTime LastLogin { get => _LastLogin; set { if (OnPropertyChanging("LastLogin", value)) { _LastLogin = value; OnPropertyChanged("LastLogin"); } } }

    private String _LastLoginIP;
    /// <summary>最后登录IP</summary>
    [Category("登录信息")]
    [DisplayName("最后登录IP")]
    [Description("最后登录IP")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("LastLoginIP", "最后登录IP", "")]
    public String LastLoginIP { get => _LastLoginIP; set { if (OnPropertyChanging("LastLoginIP", value)) { _LastLoginIP = value; OnPropertyChanged("LastLoginIP"); } } }

    private DateTime _RegisterTime;
    /// <summary>注册时间</summary>
    [Category("登录信息")]
    [DisplayName("注册时间")]
    [Description("注册时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("RegisterTime", "注册时间", "")]
    public DateTime RegisterTime { get => _RegisterTime; set { if (OnPropertyChanging("RegisterTime", value)) { _RegisterTime = value; OnPropertyChanged("RegisterTime"); } } }

    private String _RegisterIP;
    /// <summary>注册IP</summary>
    [Category("登录信息")]
    [DisplayName("注册IP")]
    [Description("注册IP")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("RegisterIP", "注册IP", "")]
    public String RegisterIP { get => _RegisterIP; set { if (OnPropertyChanging("RegisterIP", value)) { _RegisterIP = value; OnPropertyChanged("RegisterIP"); } } }

    private Int32 _OnlineTime;
    /// <summary>在线时间。累计在线总时间，秒</summary>
    [Category("登录信息")]
    [DisplayName("在线时间")]
    [Description("在线时间。累计在线总时间，秒")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("OnlineTime", "在线时间。累计在线总时间，秒", "")]
    public Int32 OnlineTime { get => _OnlineTime; set { if (OnPropertyChanging("OnlineTime", value)) { _OnlineTime = value; OnPropertyChanged("OnlineTime"); } } }

    private Int32 _Ex1;
    /// <summary>扩展1</summary>
    [Category("扩展")]
    [DisplayName("扩展1")]
    [Description("扩展1")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Ex1", "扩展1", "")]
    public Int32 Ex1 { get => _Ex1; set { if (OnPropertyChanging("Ex1", value)) { _Ex1 = value; OnPropertyChanged("Ex1"); } } }

    private Int32 _Ex2;
    /// <summary>扩展2</summary>
    [Category("扩展")]
    [DisplayName("扩展2")]
    [Description("扩展2")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Ex2", "扩展2", "")]
    public Int32 Ex2 { get => _Ex2; set { if (OnPropertyChanging("Ex2", value)) { _Ex2 = value; OnPropertyChanged("Ex2"); } } }

    private Double _Ex3;
    /// <summary>扩展3</summary>
    [Category("扩展")]
    [DisplayName("扩展3")]
    [Description("扩展3")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Ex3", "扩展3", "")]
    public Double Ex3 { get => _Ex3; set { if (OnPropertyChanging("Ex3", value)) { _Ex3 = value; OnPropertyChanged("Ex3"); } } }

    private String _Ex4;
    /// <summary>扩展4</summary>
    [Category("扩展")]
    [DisplayName("扩展4")]
    [Description("扩展4")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Ex4", "扩展4", "")]
    public String Ex4 { get => _Ex4; set { if (OnPropertyChanging("Ex4", value)) { _Ex4 = value; OnPropertyChanged("Ex4"); } } }

    private String _Ex5;
    /// <summary>扩展5</summary>
    [Category("扩展")]
    [DisplayName("扩展5")]
    [Description("扩展5")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Ex5", "扩展5", "")]
    public String Ex5 { get => _Ex5; set { if (OnPropertyChanging("Ex5", value)) { _Ex5 = value; OnPropertyChanged("Ex5"); } } }

    private String _Ex6;
    /// <summary>扩展6</summary>
    [XmlIgnore, ScriptIgnore, IgnoreDataMember]
    [Category("扩展")]
    [DisplayName("扩展6")]
    [Description("扩展6")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Ex6", "扩展6", "")]
    public String Ex6 { get => _Ex6; set { if (OnPropertyChanging("Ex6", value)) { _Ex6 = value; OnPropertyChanged("Ex6"); } } }

    private String _UpdateUser;
    /// <summary>更新者</summary>
    [Category("扩展")]
    [DisplayName("更新者")]
    [Description("更新者")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("UpdateUser", "更新者", "")]
    public String UpdateUser { get => _UpdateUser; set { if (OnPropertyChanging("UpdateUser", value)) { _UpdateUser = value; OnPropertyChanged("UpdateUser"); } } }

    private Int32 _UpdateUserID;
    /// <summary>更新用户</summary>
    [Category("扩展")]
    [DisplayName("更新用户")]
    [Description("更新用户")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("UpdateUserID", "更新用户", "")]
    public Int32 UpdateUserID { get => _UpdateUserID; set { if (OnPropertyChanging("UpdateUserID", value)) { _UpdateUserID = value; OnPropertyChanged("UpdateUserID"); } } }

    private String _UpdateIP;
    /// <summary>更新地址</summary>
    [Category("扩展")]
    [DisplayName("更新地址")]
    [Description("更新地址")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("UpdateIP", "更新地址", "")]
    public String UpdateIP { get => _UpdateIP; set { if (OnPropertyChanging("UpdateIP", value)) { _UpdateIP = value; OnPropertyChanged("UpdateIP"); } } }

    private DateTime _UpdateTime;
    /// <summary>更新时间</summary>
    [Category("扩展")]
    [DisplayName("更新时间")]
    [Description("更新时间")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("UpdateTime", "更新时间", "")]
    public DateTime UpdateTime { get => _UpdateTime; set { if (OnPropertyChanging("UpdateTime", value)) { _UpdateTime = value; OnPropertyChanged("UpdateTime"); } } }

    private String _Remark;
    /// <summary>备注</summary>
    [Category("扩展")]
    [DisplayName("备注")]
    [Description("备注")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("Remark", "备注", "")]
    public String Remark { get => _Remark; set { if (OnPropertyChanging("Remark", value)) { _Remark = value; OnPropertyChanged("Remark"); } } }
    #endregion

    #region 获取/设置 字段值
    /// <summary>获取/设置 字段值</summary>
    /// <param name="name">字段名</param>
    /// <returns></returns>
    public override Object this[String name]
    {
        get
        {
            switch (name)
            {
                case "ID": return _ID;
                case "Name": return _Name;
                case "Password": return _Password;
                case "DisplayName": return _DisplayName;
                case "Sex": return _Sex;
                case "Mail": return _Mail;
                case "Mobile": return _Mobile;
                case "Code": return _Code;
                case "AreaId": return _AreaId;
                case "Avatar": return _Avatar;
                case "RoleID": return _RoleID;
                case "RoleIds": return _RoleIds;
                case "DepartmentID": return _DepartmentID;
                case "Online": return _Online;
                case "Enable": return _Enable;
                case "Age": return _Age;
                case "Birthday": return _Birthday;
                case "Logins": return _Logins;
                case "LastLogin": return _LastLogin;
                case "LastLoginIP": return _LastLoginIP;
                case "RegisterTime": return _RegisterTime;
                case "RegisterIP": return _RegisterIP;
                case "OnlineTime": return _OnlineTime;
                case "Ex1": return _Ex1;
                case "Ex2": return _Ex2;
                case "Ex3": return _Ex3;
                case "Ex4": return _Ex4;
                case "Ex5": return _Ex5;
                case "Ex6": return _Ex6;
                case "UpdateUser": return _UpdateUser;
                case "UpdateUserID": return _UpdateUserID;
                case "UpdateIP": return _UpdateIP;
                case "UpdateTime": return _UpdateTime;
                case "Remark": return _Remark;
                default: return base[name];
            }
        }
        set
        {
            switch (name)
            {
                case "ID": _ID = value.ToInt(); break;
                case "Name": _Name = Convert.ToString(value); break;
                case "Password": _Password = Convert.ToString(value); break;
                case "DisplayName": _DisplayName = Convert.ToString(value); break;
                case "Sex": _Sex = (XCode.Membership.SexKinds)value.ToInt(); break;
                case "Mail": _Mail = Convert.ToString(value); break;
                case "Mobile": _Mobile = Convert.ToString(value); break;
                case "Code": _Code = Convert.ToString(value); break;
                case "AreaId": _AreaId = value.ToInt(); break;
                case "Avatar": _Avatar = Convert.ToString(value); break;
                case "RoleID": _RoleID = value.ToInt(); break;
                case "RoleIds": _RoleIds = Convert.ToString(value); break;
                case "DepartmentID": _DepartmentID = value.ToInt(); break;
                case "Online": _Online = value.ToBoolean(); break;
                case "Enable": _Enable = value.ToBoolean(); break;
                case "Age": _Age = value.ToInt(); break;
                case "Birthday": _Birthday = value.ToDateTime(); break;
                case "Logins": _Logins = value.ToInt(); break;
                case "LastLogin": _LastLogin = value.ToDateTime(); break;
                case "LastLoginIP": _LastLoginIP = Convert.ToString(value); break;
                case "RegisterTime": _RegisterTime = value.ToDateTime(); break;
                case "RegisterIP": _RegisterIP = Convert.ToString(value); break;
                case "OnlineTime": _OnlineTime = value.ToInt(); break;
                case "Ex1": _Ex1 = value.ToInt(); break;
                case "Ex2": _Ex2 = value.ToInt(); break;
                case "Ex3": _Ex3 = value.ToDouble(); break;
                case "Ex4": _Ex4 = Convert.ToString(value); break;
                case "Ex5": _Ex5 = Convert.ToString(value); break;
                case "Ex6": _Ex6 = Convert.ToString(value); break;
                case "UpdateUser": _UpdateUser = Convert.ToString(value); break;
                case "UpdateUserID": _UpdateUserID = value.ToInt(); break;
                case "UpdateIP": _UpdateIP = Convert.ToString(value); break;
                case "UpdateTime": _UpdateTime = value.ToDateTime(); break;
                case "Remark": _Remark = Convert.ToString(value); break;
                default: base[name] = value; break;
            }
        }
    }
    #endregion

    #region 字段名
    /// <summary>取得用户字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field ID = FindByName("ID");

        /// <summary>名称。登录用户名</summary>
        public static readonly Field Name = FindByName("Name");

        /// <summary>密码</summary>
        public static readonly Field Password = FindByName("Password");

        /// <summary>昵称</summary>
        public static readonly Field DisplayName = FindByName("DisplayName");

        /// <summary>性别。未知、男、女</summary>
        public static readonly Field Sex = FindByName("Sex");

        /// <summary>邮件</summary>
        public static readonly Field Mail = FindByName("Mail");

        /// <summary>手机</summary>
        public static readonly Field Mobile = FindByName("Mobile");

        /// <summary>代码。身份证、员工编号等</summary>
        public static readonly Field Code = FindByName("Code");

        /// <summary>地区。省市区</summary>
        public static readonly Field AreaId = FindByName("AreaId");

        /// <summary>头像</summary>
        public static readonly Field Avatar = FindByName("Avatar");

        /// <summary>角色。主要角色</summary>
        public static readonly Field RoleID = FindByName("RoleID");

        /// <summary>角色组。次要角色集合</summary>
        public static readonly Field RoleIds = FindByName("RoleIds");

        /// <summary>部门。组织机构</summary>
        public static readonly Field DepartmentID = FindByName("DepartmentID");

        /// <summary>在线</summary>
        public static readonly Field Online = FindByName("Online");

        /// <summary>启用</summary>
        public static readonly Field Enable = FindByName("Enable");

        /// <summary>年龄。周岁</summary>
        public static readonly Field Age = FindByName("Age");

        /// <summary>生日。公历年月日</summary>
        public static readonly Field Birthday = FindByName("Birthday");

        /// <summary>登录次数</summary>
        public static readonly Field Logins = FindByName("Logins");

        /// <summary>最后登录</summary>
        public static readonly Field LastLogin = FindByName("LastLogin");

        /// <summary>最后登录IP</summary>
        public static readonly Field LastLoginIP = FindByName("LastLoginIP");

        /// <summary>注册时间</summary>
        public static readonly Field RegisterTime = FindByName("RegisterTime");

        /// <summary>注册IP</summary>
        public static readonly Field RegisterIP = FindByName("RegisterIP");

        /// <summary>在线时间。累计在线总时间，秒</summary>
        public static readonly Field OnlineTime = FindByName("OnlineTime");

        /// <summary>扩展1</summary>
        public static readonly Field Ex1 = FindByName("Ex1");

        /// <summary>扩展2</summary>
        public static readonly Field Ex2 = FindByName("Ex2");

        /// <summary>扩展3</summary>
        public static readonly Field Ex3 = FindByName("Ex3");

        /// <summary>扩展4</summary>
        public static readonly Field Ex4 = FindByName("Ex4");

        /// <summary>扩展5</summary>
        public static readonly Field Ex5 = FindByName("Ex5");

        /// <summary>扩展6</summary>
        public static readonly Field Ex6 = FindByName("Ex6");

        /// <summary>更新者</summary>
        public static readonly Field UpdateUser = FindByName("UpdateUser");

        /// <summary>更新用户</summary>
        public static readonly Field UpdateUserID = FindByName("UpdateUserID");

        /// <summary>更新地址</summary>
        public static readonly Field UpdateIP = FindByName("UpdateIP");

        /// <summary>更新时间</summary>
        public static readonly Field UpdateTime = FindByName("UpdateTime");

        /// <summary>备注</summary>
        public static readonly Field Remark = FindByName("Remark");

        static Field FindByName(String name) => Meta.Table.FindByName(name);
    }

    /// <summary>取得用户字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String ID = "ID";

        /// <summary>名称。登录用户名</summary>
        public const String Name = "Name";

        /// <summary>密码</summary>
        public const String Password = "Password";

        /// <summary>昵称</summary>
        public const String DisplayName = "DisplayName";

        /// <summary>性别。未知、男、女</summary>
        public const String Sex = "Sex";

        /// <summary>邮件</summary>
        public const String Mail = "Mail";

        /// <summary>手机</summary>
        public const String Mobile = "Mobile";

        /// <summary>代码。身份证、员工编号等</summary>
        public const String Code = "Code";

        /// <summary>地区。省市区</summary>
        public const String AreaId = "AreaId";

        /// <summary>头像</summary>
        public const String Avatar = "Avatar";

        /// <summary>角色。主要角色</summary>
        public const String RoleID = "RoleID";

        /// <summary>角色组。次要角色集合</summary>
        public const String RoleIds = "RoleIds";

        /// <summary>部门。组织机构</summary>
        public const String DepartmentID = "DepartmentID";

        /// <summary>在线</summary>
        public const String Online = "Online";

        /// <summary>启用</summary>
        public const String Enable = "Enable";

        /// <summary>年龄。周岁</summary>
        public const String Age = "Age";

        /// <summary>生日。公历年月日</summary>
        public const String Birthday = "Birthday";

        /// <summary>登录次数</summary>
        public const String Logins = "Logins";

        /// <summary>最后登录</summary>
        public const String LastLogin = "LastLogin";

        /// <summary>最后登录IP</summary>
        public const String LastLoginIP = "LastLoginIP";

        /// <summary>注册时间</summary>
        public const String RegisterTime = "RegisterTime";

        /// <summary>注册IP</summary>
        public const String RegisterIP = "RegisterIP";

        /// <summary>在线时间。累计在线总时间，秒</summary>
        public const String OnlineTime = "OnlineTime";

        /// <summary>扩展1</summary>
        public const String Ex1 = "Ex1";

        /// <summary>扩展2</summary>
        public const String Ex2 = "Ex2";

        /// <summary>扩展3</summary>
        public const String Ex3 = "Ex3";

        /// <summary>扩展4</summary>
        public const String Ex4 = "Ex4";

        /// <summary>扩展5</summary>
        public const String Ex5 = "Ex5";

        /// <summary>扩展6</summary>
        public const String Ex6 = "Ex6";

        /// <summary>更新者</summary>
        public const String UpdateUser = "UpdateUser";

        /// <summary>更新用户</summary>
        public const String UpdateUserID = "UpdateUserID";

        /// <summary>更新地址</summary>
        public const String UpdateIP = "UpdateIP";

        /// <summary>更新时间</summary>
        public const String UpdateTime = "UpdateTime";

        /// <summary>备注</summary>
        public const String Remark = "Remark";
    }
    #endregion
}

/// <summary>管理员</summary>
/// <remarks>
/// 基础实体类应该是只有一个泛型参数的，需要用到别的类型时，可以继承一个，也可以通过虚拟重载等手段让基类实现
/// </remarks>
public partial class User : LogEntity<User>, IUser, IAuthUser, IIdentity
{
    #region 对象操作
    static User()
    {
        //// 用于引发基类的静态构造函数
        //var entity = new TEntity();

        //!!! 曾经这里导致产生死锁
        // 这里是静态构造函数，访问Factory引发EntityFactory.CreateOperate，
        // 里面的EnsureInit会等待实体类实例化完成，实体类的静态构造函数还卡在这里
        // 不过这不是理由，同一个线程遇到同一个锁不会堵塞
        // 发生死锁的可能性是这里引发EnsureInit，而另一个线程提前引发EnsureInit拿到锁
        var df = Meta.Factory.AdditionalFields;
        df.Add(__.Logins);
        //df.Add(__.OnlineTime);
        //Meta.Factory.FullInsert = false;

        // 单对象缓存
        var sc = Meta.SingleCache;
        sc.FindSlaveKeyMethod = k => Find(__.Name, k);
        sc.GetSlaveKeyMethod = e => e.Name;

        Meta.Modules.Add<UserModule>();
        Meta.Modules.Add<TimeModule>();
        Meta.Modules.Add<IPModule>();
    }

    /// <summary>首次连接数据库时初始化数据，仅用于实体类重载，用户不应该调用该方法</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal protected override void InitData()
    {
        if (Meta.Count > 0) return;

        if (XTrace.Debug) XTrace.WriteLine("开始初始化{0}用户数据……", typeof(User).Name);

        Add("admin", null, 1, "管理员");
        //Add("poweruser", null, 2, "高级用户");
        //Add("user", null, 3, "普通用户");

        if (XTrace.Debug) XTrace.WriteLine("完成初始化{0}用户数据！", typeof(User).Name);
    }

    /// <summary>验证</summary>
    /// <param name="isNew"></param>
    public override void Valid(Boolean isNew)
    {
        base.Valid(isNew);

        if (Name.IsNullOrEmpty()) throw new ArgumentNullException(__.Name, "用户名不能为空！");
        //if (RoleID < 1) throw new ArgumentNullException(__.RoleID, "没有指定角色！");

        //var pass = Password;
        //if (isNew)
        //{
        //    if (!pass.IsNullOrEmpty() && pass.Length != 32) Password = pass.MD5();
        //}
        //else
        //{
        //    // 编辑修改密码
        //    if (IsDirty(__.Password))
        //    {
        //        if (!pass.IsNullOrEmpty())
        //            Password = pass.MD5();
        //        else
        //            Dirtys[__.Password] = false;
        //    }
        //}

        // 重新整理角色
        var ids = GetRoleIDs();
        if (ids.Length > 0)
        {
            RoleID = ids[0];
            var str = ids.Skip(1).Join();
            if (!str.IsNullOrEmpty()) str = "," + str + ",";
            RoleIds = str;
        }

        // 自动计算年龄
        if (Birthday.Year > 1000) Age = (Int32)((DateTime.Now - Birthday).TotalDays / 365);

        //if (AreaId <= 0) FixArea(LastLoginIP);
    }

    /// <summary>删除用户</summary>
    /// <returns></returns>
    protected override Int32 OnDelete()
    {
        if (Meta.Count <= 1 && FindCount() <= 1) throw new Exception("必须保留至少一个可用账号！");

        return base.OnDelete();
    }
    #endregion

    #region 扩展属性
    /// <summary>物理地址</summary>
    [DisplayName("物理地址")]
    public String LastLoginAddress => LastLoginIP.IPToAddress();

    /// <summary>部门</summary>
    [XmlIgnore, ScriptIgnore, IgnoreDataMember]
    public Department Department => Extends.Get(nameof(Department), k => Department.FindByID(DepartmentID));

    /// <summary>部门</summary>
    [Category("登录信息")]
    [Map(__.DepartmentID, typeof(Department), __.ID)]
    public String DepartmentName => Department?.Path;

    /// <summary>
    /// 地区名
    /// </summary>
    [Map(nameof(AreaId))]
    public String AreaName => Area.FindByID(AreaId)?.Path;

    ///// <summary>兼容旧版角色组</summary>
    //[Obsolete("=>RoleIds")]
    //public String RoleIDs { get => RoleIds; set => RoleIds = value; }
    #endregion

    #region 扩展查询
    /// <summary>根据编号查找</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static User FindByID(Int32 id)
    {
        if (id <= 0) return null;

        if (Meta.Count < 1000) return Meta.Cache.Find(e => e.ID == id);

        // 实体缓存
        return Meta.SingleCache[id];
    }

    /// <summary>根据名称查找</summary>
    /// <param name="name">名称</param>
    /// <returns></returns>
    public static User FindByName(String name)
    {
        if (name.IsNullOrEmpty()) return null;

        if (Meta.Count < 1000) return Meta.Cache.Find(e => e.Name.EqualIgnoreCase(name));

        // 单对象缓存
        return Meta.SingleCache.GetItemWithSlaveKey(name) as User;
    }

    /// <summary>根据邮箱地址查找</summary>
    /// <param name="mail"></param>
    /// <returns></returns>
    public static User FindByMail(String mail)
    {
        if (mail.IsNullOrEmpty()) return null;

        if (Meta.Count < 1000) return Meta.Cache.Find(e => e.Mail.EqualIgnoreCase(mail));

        return Find(__.Mail, mail);
    }

    /// <summary>根据手机号码查找</summary>
    /// <param name="mobile"></param>
    /// <returns></returns>
    public static User FindByMobile(String mobile)
    {
        if (mobile.IsNullOrEmpty()) return null;

        if (Meta.Count < 1000) return Meta.Cache.Find(e => e.Mobile == mobile);

        return Find(__.Mobile, mobile);
    }

    /// <summary>根据唯一代码查找</summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static User FindByCode(String code)
    {
        if (code.IsNullOrEmpty()) return null;

        if (Meta.Count < 1000) return Meta.Cache.Find(e => e.Code.EqualIgnoreCase(code));

        return Find(__.Code, code);
    }

    /// <summary>为登录而查找账号，搜索名称、邮箱、手机、代码</summary>
    /// <param name="account"></param>
    /// <returns></returns>
    public static User FindForLogin(String account)
    {
        if (account.IsNullOrEmpty()) return null;

        var user = Find(_.Name == account);
        if (user == null && account.Contains("@")) user = Find(_.Mail == account);
        if (user == null && account.ToLong() > 0) user = Find(_.Mobile == account);
        if (user == null) user = Find(_.Code == account);

        return user;
    }
    #endregion

    #region 高级查询
    /// <summary>高级查询</summary>
    /// <param name="key"></param>
    /// <param name="roleId"></param>
    /// <param name="isEnable"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IList<User> Search(String key, Int32 roleId, Boolean? isEnable, PageParameter p) => Search(key, roleId, isEnable, DateTime.MinValue, DateTime.MinValue, p);

    /// <summary>高级查询</summary>
    /// <param name="key"></param>
    /// <param name="roleId"></param>
    /// <param name="isEnable"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IList<User> Search(String key, Int32 roleId, Boolean? isEnable, DateTime start, DateTime end, PageParameter p)
    {
        var exp = _.LastLogin.Between(start, end);
        if (roleId > 0) exp &= _.RoleID == roleId | _.RoleIds.Contains("," + roleId + ",");
        if (isEnable != null) exp &= _.Enable == isEnable;

        // 先精确查询，再模糊
        if (!key.IsNullOrEmpty())
        {
            var list = FindAll(exp & (_.Code == key | _.Name == key | _.DisplayName == key | _.Mail == key | _.Mobile == key), p);
            if (list.Count > 0) return list;

            exp &= (_.Code.Contains(key) | _.Name.Contains(key) | _.DisplayName.Contains(key) | _.Mail.Contains(key) | _.Mobile.Contains(key));
        }

        return FindAll(exp, p);
    }

    /// <summary>高级搜索</summary>
    /// <param name="roleId">角色</param>
    /// <param name="departmentId">部门</param>
    /// <param name="enable">启用</param>
    /// <param name="start">登录时间开始</param>
    /// <param name="end">登录时间结束</param>
    /// <param name="key">关键字，搜索代码、名称、昵称、手机、邮箱</param>
    /// <param name="page"></param>
    /// <returns></returns>
    public static IList<User> Search(Int32 roleId, Int32 departmentId, Boolean? enable, DateTime start, DateTime end, String key, PageParameter page)
    {
        var exp = new WhereExpression();
        if (roleId >= 0) exp &= _.RoleID == roleId | _.RoleIds.Contains("," + roleId + ",");
        if (departmentId >= 0) exp &= _.DepartmentID == departmentId;
        if (enable != null) exp &= _.Enable == enable.Value;
        exp &= _.LastLogin.Between(start, end);
        if (!key.IsNullOrEmpty()) exp &= _.Code.StartsWith(key) | _.Name.StartsWith(key) | _.DisplayName.StartsWith(key) | _.Mobile.StartsWith(key) | _.Mail.StartsWith(key);

        return FindAll(exp, page);
    }

    /// <summary>高级搜索</summary>
    /// <param name="roleIds">角色</param>
    /// <param name="departmentIds">部门</param>
    /// <param name="enable">启用</param>
    /// <param name="start">登录时间开始</param>
    /// <param name="end">登录时间结束</param>
    /// <param name="key">关键字，搜索代码、名称、昵称、手机、邮箱</param>
    /// <param name="page"></param>
    /// <returns></returns>
    public static IList<User> Search(Int32[] roleIds, Int32[] departmentIds, Boolean? enable, DateTime start, DateTime end, String key, PageParameter page) => Search(roleIds, departmentIds, null, enable, start, end, key, page);

    /// <summary>高级搜索</summary>
    /// <param name="roleIds">角色</param>
    /// <param name="departmentIds">部门</param>
    /// <param name="areaIds">地区</param>
    /// <param name="enable">启用</param>
    /// <param name="start">登录时间开始</param>
    /// <param name="end">登录时间结束</param>
    /// <param name="key">关键字，搜索代码、名称、昵称、手机、邮箱</param>
    /// <param name="page"></param>
    /// <returns></returns>
    public static IList<User> Search(Int32[] roleIds, Int32[] departmentIds, Int32[] areaIds, Boolean? enable, DateTime start, DateTime end, String key, PageParameter page)
    {
        var exp = new WhereExpression();
        if (roleIds != null && roleIds.Length > 0)
        {
            //exp &= _.RoleID.In(roleIds) | _.RoleIds.Contains("," + roleIds.Join(",") + ",");
            var exp2 = new WhereExpression();
            exp2 |= _.RoleID.In(roleIds);
            foreach (var rid in roleIds)
            {
                exp2 |= _.RoleIds.Contains("," + rid + ",");
            }
            exp &= exp2;
        }
        if (departmentIds != null && departmentIds.Length > 0) exp &= _.DepartmentID.In(departmentIds);
        if (areaIds != null && areaIds.Length > 0) exp &= _.AreaId.In(areaIds);
        if (enable != null) exp &= _.Enable == enable.Value;
        exp &= _.LastLogin.Between(start, end);
        if (!key.IsNullOrEmpty()) exp &= _.Code.StartsWith(key) | _.Name.StartsWith(key) | _.DisplayName.StartsWith(key) | _.Mobile.StartsWith(key) | _.Mail.StartsWith(key);

        return FindAll(exp, page);
    }
    #endregion

    #region 扩展操作
    /// <summary>添加用户，如果存在则直接返回</summary>
    /// <param name="name"></param>
    /// <param name="pass"></param>
    /// <param name="roleid"></param>
    /// <param name="display"></param>
    /// <returns></returns>
    public static User Add(String name, String pass, Int32 roleid = 1, String display = null)
    {
        //var entity = Find(_.Name == name);
        //if (entity != null) return entity;

        if (pass.IsNullOrEmpty()) pass = name;

        var entity = new User
        {
            Name = name,
            Password = pass.MD5(),
            DisplayName = display,
            RoleID = roleid,
            Enable = true
        };

        entity.Save();

        return entity;
    }

    /// <summary>已重载。显示友好名字</summary>
    /// <returns></returns>
    public override String ToString() => DisplayName.IsNullOrEmpty() ? Name : DisplayName;

    /// <summary>
    /// 修正地区
    /// </summary>
    public void FixArea(String ip)
    {
        if (ip.IsNullOrEmpty()) return;

        var list = Area.SearchIP(ip);
        if (list.Count > 0) AreaId = list[list.Count - 1].ID;
    }
    #endregion

    #region 业务
    /// <summary>登录。借助回调来验证密码</summary>
    /// <param name="username"></param>
    /// <param name="onValid"></param>
    /// <returns></returns>
    public static User Login(String username, Action<User> onValid)
    {
        if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
        if (onValid == null) throw new ArgumentNullException(nameof(onValid));

        try
        {
            // 过滤帐号中的空格，防止出现无操作无法登录的情况
            var account = username.Trim();
            //var user = FindByName(account);
            // 登录时必须从数据库查找用户，缓存中的用户对象密码字段可能为空
            var user = Find(__.Name, account);
            if (user == null) throw new EntityException("帐号{0}不存在！", account);

            if (!user.Enable) throw new EntityException("账号{0}被禁用！", account);

            // 验证用户
            onValid(user);

            user.SaveLoginInfo();

            WriteLog("登录", true, username);

            return user;
        }
        catch (Exception ex)
        {
            WriteLog("登录", false, username + "登录失败！" + ex.Message);
            throw;
        }
    }

    /// <summary>登录</summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="rememberme">是否记住密码</param>
    /// <returns></returns>
    public static User Login(String username, String password, Boolean rememberme = false)
    {
        if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
        //if (String.IsNullOrEmpty(password)) throw new ArgumentNullException("password");

        try
        {
            return Login(username, password, 1);
        }
        catch (Exception ex)
        {
            WriteLog("登录", false, username + "登录失败！" + ex.Message);
            throw;
        }
    }

    static User Login(String username, String password, Int32 hashTimes)
    {
        if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username), "该帐号不存在！");

        // 过滤帐号中的空格，防止出现无操作无法登录的情况
        var account = username.Trim();
        //var user = FindByName(account);
        // 登录时必须从数据库查找用户，缓存中的用户对象密码字段可能为空
        var user = Find(__.Name, account);
        if (user == null) throw new EntityException("帐号{0}不存在！", account);

        if (!user.Enable) throw new EntityException("账号{0}被禁用！", account);

        // 数据库为空密码，任何密码均可登录
        if (!String.IsNullOrEmpty(user.Password))
        {
            if (hashTimes > 0)
            {
                var p = password;
                if (!String.IsNullOrEmpty(p))
                {
                    for (var i = 0; i < hashTimes; i++)
                    {
                        p = p.MD5();
                    }
                }
                if (!p.EqualIgnoreCase(user.Password)) throw new EntityException("密码不正确！");
            }
            else
            {
                var p = user.Password;
                for (var i = 0; i > hashTimes; i--)
                {
                    p = p.MD5();
                }
                if (!p.EqualIgnoreCase(password)) throw new EntityException("密码不正确！");
            }
        }
        else
        {
            if (hashTimes > 0)
            {
                var p = password;
                if (!String.IsNullOrEmpty(p))
                {
                    for (var i = 0; i < hashTimes; i++)
                    {
                        p = p.MD5();
                    }
                }
                password = p;
            }
            user.Password = password;
        }

        //Current = user;

        user.SaveLoginInfo();

        if (hashTimes == -1)
            WriteLog("自动登录", true, username);
        else
            WriteLog("登录", true, username);

        return user;
    }

    /// <summary>保存登录信息</summary>
    /// <returns></returns>
    public virtual Int32 SaveLoginInfo()
    {
        Logins++;
        LastLogin = DateTime.Now;
        var ip = ManageProvider.UserHost;
        if (!ip.IsNullOrEmpty()) LastLoginIP = ip;

        Online = true;

        if (AreaId <= 0) FixArea(LastLoginIP);

        return Update();
    }

    /// <summary>注销</summary>
    public virtual void Logout()
    {
        Online = false;
        SaveAsync();
    }

    /// <summary>注册用户。第一注册用户自动抢管理员</summary>
    public virtual void Register()
    {
        using var tran = Meta.CreateTrans();
        //!!! 第一个用户注册时，如果只有一个默认admin账号，则自动抢管理员
        if (Meta.Count < 3 && FindCount() <= 1)
        {
            var list = FindAll();
            if (list.Count == 0 || list.Count == 1 && list[0].DisableAdmin())
            {
                RoleID = 1;
                Enable = true;
            }
        }

        RegisterTime = DateTime.Now;
        RegisterIP = ManageProvider.UserHost;

        Insert();

        tran.Commit();
    }

    /// <summary>禁用默认管理员</summary>
    /// <returns></returns>
    private Boolean DisableAdmin()
    {
        if (!Name.EqualIgnoreCase("admin")) return false;
        if (!Password.EqualIgnoreCase("admin".MD5())) return false;

        Enable = false;
        RoleID = 4;

        Save();

        return true;
    }
    #endregion

    #region 权限
    /// <summary>角色</summary>
    /// <remarks>扩展属性不缓存空对象，一般来说，每个管理员都有对应的角色，如果没有，可能是在初始化</remarks>
    [XmlIgnore, ScriptIgnore, IgnoreDataMember]
    public virtual IRole Role => Extends.Get(nameof(Role), k => ManageProvider.Get<IRole>()?.FindByID(RoleID));

    /// <summary>角色集合</summary>
    [XmlIgnore, ScriptIgnore, IgnoreDataMember]
    public virtual IRole[] Roles => Extends.Get(nameof(Roles), k => GetRoleIDs().Select(e => ManageProvider.Get<IRole>()?.FindByID(e)).Where(e => e != null).ToArray());

    /// <summary>获取角色列表。主角色在前，其它角色升序在后</summary>
    /// <returns></returns>
    public virtual Int32[] GetRoleIDs()
    {
        var ids = RoleIds.SplitAsInt().OrderBy(e => e).ToList();
        if (RoleID > 0) ids.Insert(0, RoleID);

        return ids.Distinct().ToArray();
    }

    /// <summary>角色名</summary>
    [Category("登录信息")]
    [Map(__.RoleID, typeof(Role), "ID")]
    public virtual String RoleName => Role + "";

    /// <summary>角色组名</summary>
    [Map(__.RoleIds)]
    public virtual String RoleNames => Extends.Get(nameof(RoleNames), k => RoleIds.SplitAsInt().Select(e => ManageProvider.Get<IRole>()?.FindByID(e)).Where(e => e != null).Select(e => e.Name).Join());

    /// <summary>用户是否拥有当前菜单的指定权限</summary>
    /// <param name="menu">指定菜单</param>
    /// <param name="flags">是否拥有多个权限中的任意一个，或的关系。如果需要表示与的关系，可以传入一个多权限位合并</param>
    /// <returns></returns>
    public Boolean Has(IMenu menu, params PermissionFlags[] flags)
    {
        if (menu == null) throw new ArgumentNullException(nameof(menu));

        // 角色集合
        var rs = Roles;

        // 如果没有指定权限子项，则指判断是否拥有资源
        if (flags == null || flags.Length == 0) return rs.Any(r => r.Has(menu.ID));

        foreach (var item in flags)
        {
            // 如果判断None，则直接返回
            if (item == PermissionFlags.None) return true;

            // 菜单必须拥有这些权限位才行
            if (menu.Permissions.ContainsKey((Int32)item))
            {
                //// 如果判断None，则直接返回
                //if (item == PermissionFlags.None) return true;

                if (rs.Any(r => r.Has(menu.ID, item))) return true;
            }
        }

        return false;
    }
    #endregion

    #region IManageUser 成员
    /// <summary>昵称</summary>
    String IManageUser.NickName { get => DisplayName; set => DisplayName = value; }

    String IIdentity.Name => Name;

    String IIdentity.AuthenticationType => "XCode";

    Boolean IIdentity.IsAuthenticated => true;
    #endregion
}