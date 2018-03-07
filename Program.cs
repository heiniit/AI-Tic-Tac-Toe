using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    class Program
    {
        static void Main(string[] args)
        {
            // AI plays both players
            TicTacDefinition theDefinition = new TicTacDefinition();
            AI theAI = new AI(theDefinition, AI.PlayerNumberEnum.AI_PLAYS_BOTH);

            // To train the AI, play plenty of games...
            for (int i = 0; i < 100000; i++)
            {
                bool debug = (i % 2000 == 0);
                theAI.NewGame();
                State state = theDefinition.CreateFirstState();
                Action action = null;

                bool finished = false;
                while (!finished)
                {
                    action = theAI.SelectAction(state);
                    state = theDefinition.CreateNextState(state, action);
                    if (debug)
                    {
                        theDefinition.DebugPrint(state);
                        //theAI.DebugPrintActions(state);
                    }
                    finished = theDefinition.IsEndState(state);
                    theAI.EarnReward(theDefinition.Reward(state), finished?1:2);
                    state = theDefinition.TogglePlayer(state);
                }
                // After the game ended, another reward call will update also the other player
                theAI.EarnReward(theDefinition.Reward(state), 2);

                if (debug)
                    Console.WriteLine("Game {0} ended.", i+1);
            }

            Console.ReadLine();
        }
    }
}
