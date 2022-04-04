

using ILRuntime.Runtime.Enviorment;
public class HotFixFacade
{
    public ILRuntime.Runtime.Enviorment.AppDomain appdomain = new ILRuntime.Runtime.Enviorment.AppDomain();
    private static HotFixFacade _instance;
    public static HotFixFacade Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new HotFixFacade();
            }
            return _instance;
        }
    }
}
