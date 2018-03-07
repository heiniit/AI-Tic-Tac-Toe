using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    class TicTacState : State
    {
        public int[,] grid = new int[3, 3];
    }

    class TicTacAction : Action
    {
        public int row = 0;
        public int col = 0;

        public TicTacAction(int _row, int _col) { row = _row; col = _col; }
    }

    class TicTacDefinition : GameDefinition
    {
        private bool debug_print_toggle = false;

        private const double REWARD_WIN = 1000000;
        private const double REWARD_LOSE = -1000000;

        public override bool IsSameState(State _state1, State _state2)
        {
            TicTacState state1 = (TicTacState)_state1;
            TicTacState state2 = (TicTacState)_state2;
            for (int i_row = 0; i_row < 3; i_row++)
            {
                for (int i_col = 0; i_col < 3; i_col++)
                {
                    if (state1.grid[i_row, i_col] != state2.grid[i_row, i_col])
                        return false;
                }
            }
            return true;
        }

        public override List<Action> PossibleActions(State _state)
        {
            TicTacState state = (TicTacState)_state;
            List<Action> actions = new List<Action>(); 

            // Go through the grid and list empty ones as possible actions
            for (int i_row = 0; i_row < 3; i_row++)
            {
                for (int i_col = 0; i_col < 3; i_col++)
                {
                    if (state.grid[i_row, i_col] == 0)
                        actions.Add(new TicTacAction(i_row, i_col));
                }
            }

            return actions;
        }

        public override double Reward(State _state)
        {
            TicTacState state = (TicTacState)_state;

            int i_sum = 0;

            // Each row
            for (int i_row = 0; i_row < 3; i_row++)
            {
                i_sum = 0;
                for (int i_col = 0; i_col < 3; i_col++)
                    i_sum += state.grid[i_row, i_col];
                if (i_sum == 3)
                    return REWARD_WIN;
                if (i_sum == -3)
                    return REWARD_LOSE;
            }

            // Each column
            for (int i_col = 0; i_col < 3; i_col++)
            {
                i_sum = 0;
                for (int i_row = 0; i_row < 3; i_row++)
                    i_sum += state.grid[i_row, i_col];
                if (i_sum == 3)
                    return REWARD_WIN;
                if (i_sum == -3)
                    return REWARD_LOSE;
            }

            // Diagonals
            i_sum = 0;
            for (int i = 0; i < 3; i++)
                i_sum += state.grid[i, i];
            if (i_sum == 3)
                return REWARD_WIN;
            if (i_sum == -3)
                return REWARD_LOSE;

            i_sum = 0;
            for (int i = 0; i < 3; i++)
                i_sum += state.grid[i, 2-i];
            if (i_sum == 3)
                return REWARD_WIN;
            if (i_sum == -3)
                return REWARD_LOSE;

            // In case neither won, the immediate reward is small and negative.
            // That guides the strategy to use minimum amount of steps to win.
            return -0.1;
        }

        public override State CreateFirstState()
        {
            TicTacState state = new TicTacState();

            // Empty grid
            for (int i_row = 0; i_row < 3; i_row++)
            {
                for (int i_col = 0; i_col < 3; i_col++)
                {
                    state.grid[i_row, i_col] = 0;
                }
            }

/*            
            // Build example situation for testing
            state.grid[0, 0] = -1;
            state.grid[0, 1] = -1;
            state.grid[0, 2] =  1;
            state.grid[1, 0] =  1;
            state.grid[2, 0] = -1;
*/            

            return state;
        }

        public override State CreateNextState(State _state, Action _action)
        {
            TicTacState state = (TicTacState)_state;
            TicTacAction action = (TicTacAction)_action;

            // Copy existing state...
            TicTacState new_state = new TicTacState();
            for (int i_row = 0; i_row < 3; i_row++)
            {
                for (int i_col = 0; i_col < 3; i_col++)
                {
                    new_state.grid[i_row, i_col] = state.grid[i_row, i_col];
                }
            }
            // ...and set one square according to the action
            new_state.grid[action.row, action.col] = 1;

            return new_state;
        }

        public override bool IsEndState(State _state)
        {
            TicTacState state = (TicTacState)_state;

            // Game ends if the grid is full...
            bool found_empty = false;
            for (int i_row = 0; i_row < 3; i_row++)
            {
                for (int i_col = 0; i_col < 3; i_col++)
                {
                    if (state.grid[i_row, i_col] == 0)
                    {
                        found_empty = true;
                        break;
                    }
                }
                if (found_empty)
                    break;
            }
            if (!found_empty)
                return true;

            // ...or the reward is large, so other player has won
            if (Math.Abs(Reward(_state)) > 10)
                return true;

            return false;
        }

        public override State TogglePlayer(State _state)
        {
            TicTacState old_state = (TicTacState)_state;
            TicTacState new_state = new TicTacState();

            // Change the sign of each element, i.e. swap X and O
            for (int i_row = 0; i_row < 3; i_row++)
            {
                for (int i_col = 0; i_col < 3; i_col++)
                {
                    new_state.grid[i_row, i_col] = -old_state.grid[i_row, i_col];
                }
            }

            return new_state;
        }

        public void DebugPrint(State _state)
        {
            TicTacState state = (TicTacState)_state;
            
            Console.WriteLine("-----");

            for (int i_row = 0; i_row < 3; i_row++)
            {
                Console.Write(" ");
                for (int i_col = 0; i_col < 3; i_col++)
                {
                    if (state.grid[i_row, i_col] == -1)
                        Console.Write(debug_print_toggle?"O":"X");
                    else if (state.grid[i_row, i_col] == 1)
                        Console.Write(debug_print_toggle?"X":"O");
                    else
                        Console.Write(" ");
                }
                Console.Write("\n");
            }

            Console.WriteLine("-----");
            debug_print_toggle = !debug_print_toggle;
        }
    }
}
