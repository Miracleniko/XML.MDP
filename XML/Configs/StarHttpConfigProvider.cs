using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Configuration;
using XML.Core.Data;
using XML.Core.Log;
using XML.Core.Remoting;
using XML.Core.Serialization;
using XML.Models;
using XML.Services;

namespace XML.Configs;

internal class StarHttpConfigProvider : HttpConfigProvider
{
    public ConfigInfo ConfigInfo { get; set; }

    private Boolean _useWorkerId;

    protected override IDictionary<String, Object> GetAll()
    {
        try
        {
            var rs = base.GetAll();

            if (rs != null && rs.Count > 0)
            {
                var inf = Info;
                ConfigInfo = JsonHelper.Convert<ConfigInfo>(inf);

                var dic = new Dictionary<String, Object>(inf);
                dic.Remove("configs");
                XTrace.WriteLine("从配置中心加载：{0}", dic.ToJson());
            }
            
            // 接收配置中心颁发的WorkerId
            if (rs != null && rs.TryGetValue("XML.Core.WorkerId", out var wid))
            {
                if (Snowflake.GlobalWorkerId <= 0) _useWorkerId = true;

                var id = wid.ToInt();
                if (id > 0 && _useWorkerId)
                {
                    XTrace.WriteLine("配置中心为当前应用实例分配全局WorkerId={0}，保障雪花Id的唯一性", id);
                    Snowflake.GlobalWorkerId = id;
                }
            }

            return rs;
        }
        catch (ApiException ex)
        {
            if (Client is ApiHttpClient http)
                http.Log?.Error(ex + "");
        }
        catch (Exception ex)
        {
            if (Client is ApiHttpClient http)
                http.Log?.Debug("配置中心[{0}]出错 {1}", http.Source, ex);
        }

        return null;
    }

    public void Attach(ICommandClient client) => client.RegisterCommand("config/publish", DoPublish);

    private String DoPublish(String argument)
    {
        // 临时采用反射办法。后面直接调用DoRefresh
        //var timer = this.GetValue("_timer") as TimerX;
        //if (timer != null) timer.SetNext(-1);
        //this.Invoke("DoRefresh", new Object[] { null });

        DoRefresh(null);

        return "刷新配置成功";
    }
}