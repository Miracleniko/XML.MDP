using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using XML.Core.Collections;
using XML.Core.Log;
using XML.Core.Stub;
using XML.Core;
using XML.XCode.Configuration;
using XML.XCode.DataAccessLayer;

namespace XML.XCode.Membership;

/// <summary>角色</summary>
[Serializable]
[DataObject]
[Description("角色")]
[BindIndex("IU_Role_Name", true, "Name")]
[BindTable("Role", Description = "角色", ConnName = "Membership", DbType = DatabaseType.None)]
public partial class Role
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
    /// <summary>名称</summary>
    [DisplayName("名称")]
    [Description("名称")]
    [DataObjectField(false, false, false, 50)]
    [BindColumn("Name", "名称", "", Master = true)]
    public String Name { get => _Name; set { if (OnPropertyChanging("Name", value)) { _Name = value; OnPropertyChanged("Name"); } } }

    private Boolean _Enable;
    /// <summary>启用</summary>
    [DisplayName("启用")]
    [Description("启用")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Enable", "启用", "")]
    public Boolean Enable { get => _Enable; set { if (OnPropertyChanging("Enable", value)) { _Enable = value; OnPropertyChanged("Enable"); } } }

    private Boolean _IsSystem;
    /// <summary>系统。用于业务系统开发使用，不受数据权限约束，禁止修改名称或删除</summary>
    [DisplayName("系统")]
    [Description("系统。用于业务系统开发使用，不受数据权限约束，禁止修改名称或删除")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("IsSystem", "系统。用于业务系统开发使用，不受数据权限约束，禁止修改名称或删除", "")]
    public Boolean IsSystem { get => _IsSystem; set { if (OnPropertyChanging("IsSystem", value)) { _IsSystem = value; OnPropertyChanged("IsSystem"); } } }

    private String _Permission;
    /// <summary>权限。对不同资源的权限，逗号分隔，每个资源的权限子项竖线分隔</summary>
    [DisplayName("权限")]
    [Description("权限。对不同资源的权限，逗号分隔，每个资源的权限子项竖线分隔")]
    [DataObjectField(false, false, true, -1)]
    [BindColumn("Permission", "权限。对不同资源的权限，逗号分隔，每个资源的权限子项竖线分隔", "")]
    public String Permission { get => _Permission; set { if (OnPropertyChanging("Permission", value)) { _Permission = value; OnPropertyChanged("Permission"); } } }

    private Int32 _Sort;
    /// <summary>排序</summary>
    [DisplayName("排序")]
    [Description("排序")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Sort", "排序", "")]
    public Int32 Sort { get => _Sort; set { if (OnPropertyChanging("Sort", value)) { _Sort = value; OnPropertyChanged("Sort"); } } }

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
    [Category("扩展")]
    [DisplayName("扩展6")]
    [Description("扩展6")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Ex6", "扩展6", "")]
    public String Ex6 { get => _Ex6; set { if (OnPropertyChanging("Ex6", value)) { _Ex6 = value; OnPropertyChanged("Ex6"); } } }

    private String _CreateUser;
    /// <summary>创建者</summary>
    [Category("扩展")]
    [DisplayName("创建者")]
    [Description("创建者")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("CreateUser", "创建者", "")]
    public String CreateUser { get => _CreateUser; set { if (OnPropertyChanging("CreateUser", value)) { _CreateUser = value; OnPropertyChanged("CreateUser"); } } }

    private Int32 _CreateUserID;
    /// <summary>创建用户</summary>
    [Category("扩展")]
    [DisplayName("创建用户")]
    [Description("创建用户")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("CreateUserID", "创建用户", "")]
    public Int32 CreateUserID { get => _CreateUserID; set { if (OnPropertyChanging("CreateUserID", value)) { _CreateUserID = value; OnPropertyChanged("CreateUserID"); } } }

    private String _CreateIP;
    /// <summary>创建地址</summary>
    [Category("扩展")]
    [DisplayName("创建地址")]
    [Description("创建地址")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("CreateIP", "创建地址", "")]
    public String CreateIP { get => _CreateIP; set { if (OnPropertyChanging("CreateIP", value)) { _CreateIP = value; OnPropertyChanged("CreateIP"); } } }

    private DateTime _CreateTime;
    /// <summary>创建时间</summary>
    [Category("扩展")]
    [DisplayName("创建时间")]
    [Description("创建时间")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("CreateTime", "创建时间", "")]
    public DateTime CreateTime { get => _CreateTime; set { if (OnPropertyChanging("CreateTime", value)) { _CreateTime = value; OnPropertyChanged("CreateTime"); } } }

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
                case "Enable": return _Enable;
                case "IsSystem": return _IsSystem;
                case "Permission": return _Permission;
                case "Sort": return _Sort;
                case "Ex1": return _Ex1;
                case "Ex2": return _Ex2;
                case "Ex3": return _Ex3;
                case "Ex4": return _Ex4;
                case "Ex5": return _Ex5;
                case "Ex6": return _Ex6;
                case "CreateUser": return _CreateUser;
                case "CreateUserID": return _CreateUserID;
                case "CreateIP": return _CreateIP;
                case "CreateTime": return _CreateTime;
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
                case "Enable": _Enable = value.ToBoolean(); break;
                case "IsSystem": _IsSystem = value.ToBoolean(); break;
                case "Permission": _Permission = Convert.ToString(value); break;
                case "Sort": _Sort = value.ToInt(); break;
                case "Ex1": _Ex1 = value.ToInt(); break;
                case "Ex2": _Ex2 = value.ToInt(); break;
                case "Ex3": _Ex3 = value.ToDouble(); break;
                case "Ex4": _Ex4 = Convert.ToString(value); break;
                case "Ex5": _Ex5 = Convert.ToString(value); break;
                case "Ex6": _Ex6 = Convert.ToString(value); break;
                case "CreateUser": _CreateUser = Convert.ToString(value); break;
                case "CreateUserID": _CreateUserID = value.ToInt(); break;
                case "CreateIP": _CreateIP = Convert.ToString(value); break;
                case "CreateTime": _CreateTime = value.ToDateTime(); break;
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
    /// <summary>取得角色字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field ID = FindByName("ID");

        /// <summary>名称</summary>
        public static readonly Field Name = FindByName("Name");

        /// <summary>启用</summary>
        public static readonly Field Enable = FindByName("Enable");

        /// <summary>系统。用于业务系统开发使用，不受数据权限约束，禁止修改名称或删除</summary>
        public static readonly Field IsSystem = FindByName("IsSystem");

        /// <summary>权限。对不同资源的权限，逗号分隔，每个资源的权限子项竖线分隔</summary>
        public static readonly Field Permission = FindByName("Permission");

        /// <summary>排序</summary>
        public static readonly Field Sort = FindByName("Sort");

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

        /// <summary>创建者</summary>
        public static readonly Field CreateUser = FindByName("CreateUser");

        /// <summary>创建用户</summary>
        public static readonly Field CreateUserID = FindByName("CreateUserID");

        /// <summary>创建地址</summary>
        public static readonly Field CreateIP = FindByName("CreateIP");

        /// <summary>创建时间</summary>
        public static readonly Field CreateTime = FindByName("CreateTime");

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

    /// <summary>取得角色字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String ID = "ID";

        /// <summary>名称</summary>
        public const String Name = "Name";

        /// <summary>启用</summary>
        public const String Enable = "Enable";

        /// <summary>系统。用于业务系统开发使用，不受数据权限约束，禁止修改名称或删除</summary>
        public const String IsSystem = "IsSystem";

        /// <summary>权限。对不同资源的权限，逗号分隔，每个资源的权限子项竖线分隔</summary>
        public const String Permission = "Permission";

        /// <summary>排序</summary>
        public const String Sort = "Sort";

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

        /// <summary>创建者</summary>
        public const String CreateUser = "CreateUser";

        /// <summary>创建用户</summary>
        public const String CreateUserID = "CreateUserID";

        /// <summary>创建地址</summary>
        public const String CreateIP = "CreateIP";

        /// <summary>创建时间</summary>
        public const String CreateTime = "CreateTime";

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

/// <summary>角色</summary>
public partial class Role : LogEntity<Role>, IRole
{
    #region 对象操作
    static Role()
    {
        //Meta.Factory.FullInsert = false;

        Meta.Modules.Add<UserModule>();
        Meta.Modules.Add<TimeModule>();
        Meta.Modules.Add<IPModule>();
    }

    /// <summary>首次连接数据库时初始化数据，仅用于实体类重载，用户不应该调用该方法</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal protected override void InitData()
    {
        if (Meta.Count > 0)
        {
            // 必须有至少一个可用的系统角色
            //var list = Meta.Cache.Entities.ToList();
            // InitData中用缓存将会导致二次调用InitData，从而有一定几率死锁
            var list = FindAll().ToList();
            if (list.Count > 0 && !list.Any(e => e.IsSystem))
            {
                // 如果没有，让第一个角色作为系统角色
                var role = list[0];
                role.IsSystem = true;

                XTrace.WriteLine("必须有至少一个可用的系统角色，修改{0}为系统角色！", role.Name);

                role.Save();
            }
        }
        else
        {
            if (XTrace.Debug) XTrace.WriteLine("开始初始化{0}角色数据……", typeof(Role).Name);

            Add("管理员", true, "默认拥有全部最高权限，由系统工程师使用，安装配置整个系统");
            Add("高级用户", false, "业务管理人员，可以管理业务模块，可以分配授权用户等级");
            Add("普通用户", false, "普通业务人员，可以使用系统常规业务模块功能");
            Add("游客", false, "新注册默认属于游客");

            if (XTrace.Debug) XTrace.WriteLine("完成初始化{0}角色数据！", typeof(Role).Name);
        }

        //CheckRole();
        //// 当前处于事务之中，下面使用Menu会触发异步检查架构，SQLite单线程机制可能会造成死锁
        //ThreadPoolX.QueueUserWorkItem(CheckRole);
    }

    /// <summary>初始化时执行必要的权限检查，以防万一管理员无法操作</summary>
    static void CheckRole()
    {
        // InitData中用缓存将会导致二次调用InitData，从而有一定几率死锁
        var list = FindAll();

        // 如果某些菜单已经被删除，但是角色权限表仍然存在，则删除
        var menus = Menu.FindAll();
        var ids = menus.Select(e => e.ID).ToArray();
        foreach (var role in list)
        {
            if (!role.CheckValid(ids)) XTrace.WriteLine("删除[{0}]中的无效资源权限！", role);
        }

        // 所有角色都有权进入管理平台，否则无法使用后台
        var menu = menus.FirstOrDefault(e => e.Name == "Admin");
        if (menu != null)
        {
            foreach (var role in list)
            {
                role.Set(menu.ID, PermissionFlags.Detail);
            }
        }
        list.Save();

        // 系统角色
        var sys = list.Where(e => e.IsSystem).OrderBy(e => e.ID).FirstOrDefault();
        if (sys == null) return;

        // 如果没有任何角色拥有权限管理的权限，那是很悲催的事情
        var count = 0;
        foreach (var item in menus)
        {
            //if (item.Visible && !list.Any(e => e.Has(item.ID, PermissionFlags.Detail)))
            if (!list.Any(e => e.Has(item.ID, PermissionFlags.Detail)))
            {
                count++;
                sys.Set(item.ID, PermissionFlags.All);

                XTrace.WriteLine("没有任何角色拥有菜单[{0}]的权限", item.Name);
            }
        }
        if (count > 0)
        {
            XTrace.WriteLine("共有{0}个菜单，没有任何角色拥有权限，准备授权第一系统角色[{1}]拥有其完全管理权！", count, sys);
            sys.Save();

            // 更新缓存
            Meta.Cache.Clear("CheckRole", true);
        }
    }

    /// <summary>验证数据，通过抛出异常的方式提示验证失败。</summary>
    /// <param name="isNew">是否新数据</param>
    public override void Valid(Boolean isNew)
    {
        if (String.IsNullOrEmpty(Name)) throw new ArgumentNullException(__.Name, _.Name.DisplayName + "不能为空！");

        base.Valid(isNew);

        SavePermission();
    }

    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override Int32 Delete()
    {
        var entity = this;
        var name = entity.Name;
        if (String.IsNullOrEmpty(name))
        {
            entity = FindByID(ID);
            if (entity != null) name = entity.Name;
        }

        if (Meta.Count <= 1 && FindCount() <= 1)
        {
            var msg = $"至少保留一个角色[{name}]禁止删除！";
            WriteLog("删除", true, msg);

            throw new XException(msg);
        }

        if (entity.IsSystem)
        {
            var msg = $"系统角色[{name}]禁止删除！";
            WriteLog("删除", true, msg);

            throw new XException(msg);
        }

        return base.Delete();
    }

    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override Int32 Save()
    {
        // 先处理一次，否则可能因为别的字段没有修改而没有脏数据
        SavePermission();

        return base.Save();
    }

    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override Int32 Update()
    {
        // 先处理一次，否则可能因为别的字段没有修改而没有脏数据
        SavePermission();

        return base.Update();
    }

    /// <summary>加载权限字典</summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        // 构造权限字典
        LoadPermission();
    }

    /// <summary>如果Permission被修改，则重新加载</summary>
    /// <param name="fieldName"></param>
    protected override void OnPropertyChanged(String fieldName)
    {
        base.OnPropertyChanged(fieldName);

        if (fieldName == __.Permission) LoadPermission();
    }
    #endregion

    #region 扩展查询
    /// <summary>根据编号查找角色</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Role FindByID(Int32 id)
    {
        if (id <= 0 || Meta.Cache.Entities == null || Meta.Cache.Entities.Count <= 0) return null;

        return Meta.Cache.Entities.ToArray().FirstOrDefault(e => e.ID == id);
    }

    /// <summary>根据编号查找角色</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IRole IRole.FindByID(Int32 id) => FindByID(id);

    /// <summary>根据名称查找角色</summary>
    /// <param name="name">名称</param>
    /// <returns></returns>
    public static Role FindByName(String name)
    {
        if (String.IsNullOrEmpty(name) || Meta.Cache.Entities == null || Meta.Cache.Entities.Count <= 0) return null;

        return Meta.Cache.Find(e => e.Name.EqualIgnoreCase(name));
    }
    #endregion

    #region 扩展权限
    private IDictionary<Int32, PermissionFlags> _Permissions;
    /// <summary>本角色权限集合</summary>
    [XmlIgnore, ScriptIgnore, IgnoreDataMember]
    public IDictionary<Int32, PermissionFlags> Permissions => _Permissions ??= new Dictionary<Int32, PermissionFlags>();

    /// <summary>是否拥有指定资源的指定权限</summary>
    /// <param name="resid"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
    public Boolean Has(Int32 resid, PermissionFlags flag = PermissionFlags.None)
    {
        var pf = PermissionFlags.None;
        if (!Permissions.TryGetValue(resid, out pf)) return false;
        if (flag == PermissionFlags.None) return true;

        return pf.Has(flag);
    }

    void Remove(Int32 resid)
    {
        if (Permissions.ContainsKey(resid)) Permissions.Remove(resid);
    }

    /// <summary>获取权限</summary>
    /// <param name="resid"></param>
    /// <returns></returns>
    public PermissionFlags Get(Int32 resid)
    {
        if (!Permissions.TryGetValue(resid, out var pf)) return PermissionFlags.None;

        return pf;
    }

    /// <summary>设置该角色拥有指定资源的指定权限</summary>
    /// <param name="resid"></param>
    /// <param name="flag"></param>
    public void Set(Int32 resid, PermissionFlags flag = PermissionFlags.All)
    {
        if (Permissions.TryGetValue(resid, out var pf))
        {
            Permissions[resid] = pf | flag;
        }
        else
        {
            if (flag != PermissionFlags.None) Permissions.Add(resid, flag);
        }
    }

    /// <summary>重置该角色指定的权限</summary>
    /// <param name="resid"></param>
    /// <param name="flag"></param>
    public void Reset(Int32 resid, PermissionFlags flag)
    {
        if (Permissions.TryGetValue(resid, out var pf))
        {
            Permissions[resid] = pf & ~flag;
        }
    }

    /// <summary>检查是否有无效权限项，有则删除</summary>
    /// <param name="resids"></param>
    internal Boolean CheckValid(Int32[] resids)
    {
        if (resids == null || resids.Length == 0) return true;

        var ps = Permissions;
        var count = ps.Count;

        var list = new List<Int32>();
        foreach (var item in ps)
        {
            if (!resids.Contains(item.Key)) list.Add(item.Key);
        }
        // 删除无效项
        foreach (var item in list)
        {
            ps.Remove(item);
        }

        return count == ps.Count;
    }

    void LoadPermission()
    {
        Permissions.Clear();
        if (String.IsNullOrEmpty(Permission)) return;

        var dic = Permission.SplitAsDictionary("#", ",");
        foreach (var item in dic)
        {
            var resid = item.Key.ToInt();
            Permissions[resid] = (PermissionFlags)item.Value.ToInt();
        }
    }

    void SavePermission()
    {
        var ps = _Permissions;
        if (ps == null) return;

        // 不能这样子直接清空，因为可能没有任何改变，而这么做会两次改变脏数据，让系统以为有改变
        //Permission = null;
        if (ps.Count <= 0)
        {
            //Permission = null;
            SetItem(__.Permission, null);
            return;
        }

        var sb = Pool.StringBuilder.Get();
        // 根据资源按照从小到大排序一下
        foreach (var item in ps.OrderBy(e => e.Key))
        {
            //// 跳过None
            //if (item.Value == PermissionFlags.None) continue;
            // 不要跳过None，因为None表示只读

            if (sb.Length > 0) sb.Append(',');
            sb.AppendFormat("{0}#{1}", item.Key, (Int32)item.Value);
        }
        SetItem(__.Permission, sb.Put(true));
    }

    /// <summary>当前角色拥有的资源</summary>
    [XmlIgnore, ScriptIgnore, IgnoreDataMember]
    public Int32[] Resources { get { return Permissions.Keys.ToArray(); } }
    #endregion

    #region 业务
    /// <summary>根据名称查找角色，若不存在则创建</summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IRole GetOrAdd(String name)
    {
        if (name.IsNullOrEmpty()) return null;

        return Add(name, false);
    }

    /// <summary>根据名称查找角色，若不存在则创建</summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IRole IRole.GetOrAdd(String name)
    {
        if (name.IsNullOrEmpty()) return null;

        return Add(name, false);
    }

    /// <summary>添加角色，如果存在，则直接返回，否则创建</summary>
    /// <param name="name"></param>
    /// <param name="issys"></param>
    /// <param name="remark"></param>
    /// <returns></returns>
    public static Role Add(String name, Boolean issys, String remark = null)
    {
        //var entity = FindByName(name);
        var entity = Find(__.Name, name);
        if (entity != null) return entity;

        entity = new Role
        {
            Name = name,
            IsSystem = issys,
            Enable = true,
            Remark = remark
        };
        entity.Save();

        return entity;
    }
    #endregion
}