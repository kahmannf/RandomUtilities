using RandomUtilities.Extensions;
using RandomUtilities.MathUtil.Vectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.MathUtil
{
    public class LinearEquationSystem
    {
        public LinearEquationSystem() : this (new List<Equation>()){ }

        public LinearEquationSystem(IEnumerable<Equation> equations)
        {
            _equations = new List<Equation>(equations.Select(x => x.Value));
        }

        #region Static Creating-Methods

        public static LinearEquationSystem FromVStraights(V2Straight s1, char scaleS1, V2Straight s2, char scaleS2)
        {
            return new LinearEquationSystem
                (
                    new List<Equation>()
                    {
                        new Equation
                        (
                            s1.GetXSegments(scaleS1),
                            s2.GetXSegments(scaleS2)
                        ),
                        new Equation
                        (
                            s1.GetYSegments(scaleS1),
                            s2.GetYSegments(scaleS2)
                        )
                    }
                );
        }

        #endregion

        private List<Equation> _equations;

        public LinearEquationSystem Value => new LinearEquationSystem(_equations);

        public IEnumerable<char> AllUnknowns => (from equ in _equations select equ.Unknowns.Select(x => x.Unknown.Value)).Aggregate((x, y) => x.Union(y));

        public int AllUnknownsCount => AllUnknowns.Count();

        public int EquationCount => _equations.Count;

        public LinearEquationSystem Normalize()
        {
            List<char> unknowns = new List<char>(AllUnknowns);

            List<Equation> result = new List<Equation>();

            foreach (Equation equ in _equations)
            {
                result.Add(equ.Normalize(unknowns));
            }

            return new LinearEquationSystem(result);
        }

        public override string ToString()
        {
            string result = string.Empty;

            foreach (Equation equ in _equations)
            {
                result += equ + Environment.NewLine;
            }

            return result.TrimEnd(Environment.NewLine.ToCharArray());
        }

        public Equation this[int index]
        {
            get => _equations[index];
            set => _equations[index] = value;
        }

        /// <summary>
        /// Swaps the specified row with a row that whichs vale is not 0
        /// at the specified column index
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        public bool SwapRows(int row, int column)
        {
            bool swapped = false;
            for (int z = EquationCount - 1; z > row; z--)
            {
                if (this[z][row].ValueFactor != 0)
                {
                    _equations = _equations.Swap(z, column).ToList();
                    swapped = true;
                }
            }
            return swapped;
        }

        public Dictionary<char, SaveDouble> SolveGaussian()
        {
            return this.Normalize().SolveGaussianInternal();
        }

        private Dictionary<char, SaveDouble> SolveGaussianInternal()
        {

            // input checks
            int rowCount = this.EquationCount;
            if (rowCount < 1)
                throw new ArgumentException("The LinearEquationSystem must at least have one Equation.");

            // pivoting
            for (int col = 0; col + 1 < rowCount; col++)
            {
                if (this[col][col].ValueFactor == 0)
                // check for zero coefficients
                {
                    // find non-zero coefficient
                    int swapRow = col + 1;
                    for (; swapRow < rowCount; swapRow++)
                        if (this[swapRow][col].ValueFactor != 0) break;

                    if (this[swapRow][col].ValueFactor != 0) // found a non-zero coefficient?
                    {
                        // yes, then swap it with the above
                        for (int i = 0; i < rowCount + 1; i++)
                        {
                            _equations = _equations.Swap(col, swapRow).ToList();
                        }
                    }
                    else
                        return new Dictionary<char, SaveDouble>(); // no, then the matrix has no unique solution
                }
            }

            // elimination
            for (int sourceRow = 0; sourceRow + 1 < rowCount; sourceRow++)
            {
                for (int destRow = sourceRow + 1; destRow < rowCount; destRow++)
                {
                    SaveDouble df = this[sourceRow][sourceRow].ValueFactor;
                    SaveDouble sf = this[destRow][sourceRow].ValueFactor;

                    for (int i = 0; i < rowCount + 1; i++)
                        this[destRow][i].ValueFactor = this[destRow][i].ValueFactor * df - this[sourceRow][i].ValueFactor * sf;
                }
            }

            // back-insertion
            for (int row = rowCount - 1; row >= 0; row--)
            {
                SaveDouble f = this[row][row].ValueFactor;
                if (f == 0) return new Dictionary<char, SaveDouble>();

                for (int i = 0; i < rowCount + 1; i++)
                {
                    this[row][i].ValueFactor /= f;
                }

                for (int destRow = 0; destRow < row; destRow++)
                {
                    this[destRow][rowCount].ValueFactor -= this[destRow][row].ValueFactor * this[row][rowCount].ValueFactor;
                    this[destRow][row].ValueFactor = 0;
                }
            }

            Dictionary<char, SaveDouble> result = new Dictionary<char, SaveDouble>();

            for (int i = 0; i < rowCount; i++)
            {
                result.Add(this[i][i].Unknown.Value, this[i][AllUnknownsCount].ValueFactor);
            }

            return result;
        }
        private SaveDouble[,] AsNormalizedMatrix()
        {
            LinearEquationSystem system = this.Normalize();

            SaveDouble[,] result = new SaveDouble[system.EquationCount, system.AllUnknownsCount + 1];

            for (int i = 0; i < system.EquationCount; i++)
            {
                for (int j = 0; j < system.AllUnknownsCount; j++)
                {
                    result[i, j] = system[i][j].ValueFactor;
                }

                result[i, system.AllUnknownsCount] = system[i][AllUnknownsCount].ValueFactor; //Cosntant on rigth side
            }

            return result;
        }
    }
}
