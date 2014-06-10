namespace CrmScriptLister
{
    using System;

    public static class Extensions
    {
        public static TResult IfNotNull<T, TResult>(this T target, Func<T, TResult> getValue)
        {
            if (target != null)
                return getValue(target);
            else
                return default(TResult);
        }
    }
}
