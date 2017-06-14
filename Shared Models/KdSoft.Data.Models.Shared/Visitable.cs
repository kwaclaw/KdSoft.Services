
namespace KdSoft.Data.Models.Shared
{
    public interface IVisitor<T> where T: class
    {
        void Visit(T element);
    }

    /// <summary>
    /// Base class for visited elements - see Visitor pattern.
    /// A Visitor class must have a Visit() method as if it implemented the
    /// IVistor{T} interface, where T is the Visitable sub-type.
    /// </summary>
    public class Visitable
    {
        // dynamic will use runtime compiled Expressions and cache them;
        // first call is therefore slow, subsequent calls are as fast as regular delegate calls.
        public void Accept(dynamic visitor) {
            visitor.Visit((dynamic)this);  // will perform dynamic overload resolution!
        }
    }
}
