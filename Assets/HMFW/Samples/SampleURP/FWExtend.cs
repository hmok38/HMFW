using UnityEngine;

namespace HMFW.SampleURP
{
    /// <summary>
    /// 框架扩展和访问示例
    /// </summary>
    public class TestClass
    {
        public void Test()
        {
            /*----------框架中所有功能模块分为2中----------
             * 一种是框架自带的功能模块 如 GameFsmMgr -都会有一个基类,用来规范最基本的接口
             * 另外一种是业务新建立的各种管理类-如示例中的 FWTestMgr,框架内部不含它,不强制要求它必须拥有基类
             *使用如下方法是为了保证框架在进行修改和更换某些插件时,业务层接口统一,尽量少修改和不修改业务代码
             * 业务层一定要通过FW.API.的方式访问所有接口,甚至第三方插件也可以这样来访问
            */

            /*----------如果要对自带模块 新增接口或者扩展接口 ------
             *可以使用静态扩展的方式扩展接口,因为所有的模块都是单例的方式 可以参考示例中的 SampleMgrExtend静态类,
             *甚至可以控制到某一个程序集只允许访问某一个扩展接口的程度
             *访问扩展接口的方式与访问原来接口方式一样,业务中可以不用关心实现,也不用升级框架SDK
             */
            //--业务中可以直接访问这个扩展的接口
            FW.API.SampleMgr.ExtendMethod();

            
            
            /* ------------如果要跟换框架自带的模块或者修改模块的某些函数-----------
             * 可以用继承的方式:
             * 1:更换框架的话就直接继承这个模块的base类,然后重新写这个模块,只要实现了基类中所有的公共接口即可
             * 2:修改逻辑,可以继承这个模块的实现类,然后覆盖部分要修改的接口即可
             *
             * 修改完后,在使用前赋值新的就行--如下示例
             */

            //如果要更换框架中自带的模块,可以手动设置其基类的单例,那么通过相同api访问的就是新的
            Debug.Log(FW.API.SampleMgr.SampleAMethod()); //结果:1;--访问的还是SampleMgrBase的接口
            FW.API.SampleMgr = new SampleMgrNew();
            Debug.Log(FW.API.SampleMgr.SampleAMethod()); //结果:2;--访问的是 SampleMgrNew 的接口了
            //新模块的新函数无法通过api接口访问到,除非是用扩展的方式做的
            //FW.API.SampleMgr.SampleMethodB();
            //对新函数再次扩展后就可以访问了
            FW.API.SampleMgr.SampleMethodC();
            
            

            /* -----------------如果业务中需要建立新功能模块-----
             *  要建立新模块,那么可以对FW类进行扩展,请参考: FWExtend 静态类
             *  这样以后修改逻辑或者替换模块也好,业务层不需要修改代码
             */
            //---业务中访问新扩展的功能模块的方法
            FW.API.FWTestMgr().FWTestMgrMethod();
        }
    }

    /// <summary>
    /// 框架扩展新模块示例
    /// </summary>
    public static class FWExtend
    {
        /// <summary>
        /// 使用扩展的方式将新的模块扩展到 FW.API统一接口上
        /// </summary>
        /// <param name="fw"></param>
        /// <returns></returns>
        public static FWTestMgr FWTestMgr(this FW fw)
        {
            return HMFW.SampleURP.FWTestMgr.Instance;
        }
    }

    /// <summary>
    /// 测试用的新功能模块
    /// </summary>
    public class FWTestMgr
    {
        public static FWTestMgr Instance { get; set; } = new FWTestMgr();

        /// <summary>
        /// 测试用的新功能模块的接口
        /// </summary>
        public void FWTestMgrMethod()
        {
        }
    }

    /// <summary>
    /// 框架自带的模块的接口扩展示例
    /// </summary>
    public static class SampleMgrExtend
    {
        /// <summary>
        /// 扩展增加的新接口
        /// </summary>
        /// <param name="gameFsmMgr"></param>
        /// <returns></returns>
        public static int ExtendMethod(this SampleMgrBase gameFsmMgr)
        {
            return 1;
        }
    }

    /// <summary>
    /// 新的sampleMgr模块
    /// </summary>
    public class SampleMgrNew : SampleMgrBase
    {
        public override int SampleAMethod()
        {
            return 0;
        }

        /// <summary>
        /// 新模块的新函数是不能通过 FW.API.SampleMgr.SampleBMethod() 访问,必须通过扩展的方式创建的函数才可以访问
        /// </summary>
        public void SampleMethodB() //无法 FW.API.SampleMgr.SampleMethodB 访问
        {
        }

        /// <summary>
        /// 新模块未扩展新函数是不能通过 FW.API.SampleMgr.SampleCMethod() 访问,必须通过扩展的方式创建的函数才可以访问
        /// 可以再对基类扩展一次它 请参考:SampleMgrNewExtend类
        /// </summary>
        public void SampleMethodC() //未扩展的话无法 FW.API.SampleMgr.SampleMethodC 访问
        {
        }
    }

    public static class SampleMgrNewExtend
    {
        //用这样的方式就通过 FW.API.SampleMgr.SampleMethodB 访问了
        public static void SampleMethodC(this SampleMgrBase sampleMgrBase)
        {
            if (sampleMgrBase.GetType() == typeof(SampleMgrNew))
            {
                var mgr = (SampleMgrNew) sampleMgrBase;
                mgr.SampleMethodC();
            }
        }
    }
}