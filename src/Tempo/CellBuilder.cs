using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tempo.Util;

namespace Tempo
{
    /// <summary>
    /// Provides static methods for constructing cells.
    /// </summary>
    public static class CellBuilder
    {
        public static ICellRead<Unit> UnitCell()
        {
            return Const(Unit.Value);
        }

        /// <summary>
        /// Construct a constant cell, with a lifetime equal to the given scope. If the value is reference counted,
        /// it will be released when the scope ends.
        /// </summary>
        /// <typeparam name="T">The type of the constant value.</typeparam>
        /// <param name="value">The constant value.</param>
        /// <returns></returns>
        public static ICellRead<T> Const<T>(T value)
        {
            return new MemoryCell<T>(value);
        }


        /// <summary>
        /// Construct a cell derived from the values of two others.
        /// </summary>
        /// <typeparam name="T1">The type of the first source cell.</typeparam>
        /// <typeparam name="T2">The type of the second source cell.</typeparam>
        /// <typeparam name="TOut">The result type.</typeparam>
        /// <param name="c1">The first source cell.</param>
        /// <param name="c2">The second source cell.</param>
        /// <param name="selector">A function to compute the current value of the result from the source values.</param>
        /// <returns></returns>
        public static ICellRead<TOut> Merge<T1, T2, TOut>(ICellRead<T1> c1, ICellRead<T2> c2, Func<T1, T2, TOut> selector)
        {
            return new AnonymousCellRead<TOut>(
                () => selector(c1.Cur, c2.Cur),
                (lifetime, handler) =>
                {
                    c1.ListenForChanges(lifetime, handler);
                    c2.ListenForChanges(lifetime, handler);
                });
        }

        /// <summary>
        /// Construct a cell derived from the values of two others.
        /// </summary>
        /// <typeparam name="T1">The type of the first source cell.</typeparam>
        /// <typeparam name="T2">The type of the second source cell.</typeparam>
        /// <typeparam name="T3">The type of the third source cell.</typeparam>
        /// <typeparam name="TOut">The result type.</typeparam>
        /// <param name="c1">The first source cell.</param>
        /// <param name="c2">The second source cell.</param>
        /// <param name="c3">The third source cell.</param>
        /// <param name="selector">A function to compute the current value of the result from the source values.</param>
        /// <returns></returns>
        public static ICellRead<TOut> Merge<T1, T2, T3, TOut>(ICellRead<T1> c1, ICellRead<T2> c2, ICellRead<T3> c3, Func<T1, T2, T3, TOut> selector)
        {
            return new AnonymousCellRead<TOut>(
                () => selector(c1.Cur, c2.Cur, c3.Cur),
                (lifetime, handler) =>
                {
                    c1.ListenForChanges(lifetime, handler);
                    c2.ListenForChanges(lifetime, handler);
                    c3.ListenForChanges(lifetime, handler);
                });
        }

        /// <summary>
        /// Construct a cell derived from the values of four others.
        /// </summary>
        /// <typeparam name="T1">The type of the first source cell.</typeparam>
        /// <typeparam name="T2">The type of the second source cell.</typeparam>
        /// <typeparam name="T3">The type of the third source cell.</typeparam>
        /// <typeparam name="T4">The type of the fourth source cell.</typeparam>
        /// <typeparam name="TOut">The result type.</typeparam>
        /// <param name="c1">The first source cell.</param>
        /// <param name="c2">The second source cell.</param>
        /// <param name="c3">The third source cell.</param>
        /// <param name="c4">The fourth source cell.</param>
        /// <param name="selector">A function to compute the current value of the result from the source values.</param>
        /// <returns></returns>
        public static ICellRead<TOut> Merge<T1, T2, T3, T4, TOut>(ICellRead<T1> c1, ICellRead<T2> c2, ICellRead<T3> c3, ICellRead<T4> c4, Func<T1, T2, T3, T4, TOut> selector)
        {
            return new AnonymousCellRead<TOut>(
                () => selector(c1.Cur, c2.Cur, c3.Cur, c4.Cur),
                (lifetime, handler) =>
                {
                    c1.ListenForChanges(lifetime, handler);
                    c2.ListenForChanges(lifetime, handler);
                    c3.ListenForChanges(lifetime, handler);
                    c4.ListenForChanges(lifetime, handler);
                });
        }
    }
}
