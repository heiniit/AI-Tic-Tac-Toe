using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    abstract class State {}

    abstract class Action {}
   
    abstract class GameDefinition
    {
        // Check similarity of states
        abstract public bool IsSameState(State _state1, State _state2);

        // Given the state, what possible actions there are
        abstract public List<Action> PossibleActions(State _state);

        // Calculate the reward based on the resulting state
        abstract public double Reward(State _state);

        // Where to begin
        abstract public State CreateFirstState();

        // Combine state and an action to a new state
        abstract public State CreateNextState(State _state, Action _action);

        // Tell if the game has ended or should we still continue
        abstract public bool IsEndState(State _state);

        // If AI is used to play both players, this is used to change sides (e.g. change X to O and O to X) 
        abstract public State TogglePlayer(State _state);
    }

    class AI
    {
        public enum PlayerNumberEnum
        {
            AI_PLAYS_SINGLE,
            AI_PLAYS_BOTH
        };

        private GameDefinition def = null;
        private PlayerNumberEnum player_number = PlayerNumberEnum.AI_PLAYS_BOTH;
        private Random rnd = new Random();

        // Private structure containing known states, possible actions and expectation values
        private class ActionStruct
        {
            public Action action { get; set; }
            public StateStruct parent { get; set; }
            public double expectation { get; set; }
            public int samples { get; set; }

            public ActionStruct(Action _action, StateStruct _parent)
            {
                parent = _parent;
                action = _action;
                expectation = 100;
                samples = 0;
            }
        }

        private class StateStruct
        {
            public State state { get; set; }
            public List<ActionStruct> actions;

            // When a new struct is created, possible actions are asked from game definition
            public StateStruct(State _state, GameDefinition _def)
            {
                state = _state;
                actions = new List<ActionStruct>();
                List<Action> possible_actions = _def.PossibleActions(_state);
                foreach (Action action in possible_actions)
                {
                    actions.Add(new ActionStruct(action, this));
                }
            }
        }
    
        // The struccture that holds collected data, what have been learned
        private List<StateStruct> states = new List<StateStruct>();

        // Previous selected actions, i.e. what should be updated once we get the reward
        private List<ActionStruct> last_actions = new List<ActionStruct>();

        public AI(GameDefinition _def, PlayerNumberEnum _pn)
        {
            def = _def;
            player_number = _pn;
        }

        public void NewGame()
        {
            // Reset previous actions (but remember known states and their values)
            last_actions.RemoveAll(x => true);
        }

        // The beef: return an optimal action when we know the state
        public Action SelectAction(State _state)
        {
            // Find corresponding state struct
            StateStruct found = null;
            foreach (StateStruct statestr in states)
            {
                if (def.IsSameState(statestr.state, _state))
                {
                    found = statestr;
                    break;
                }
            }
            // If not found, we have a new state, let's add that
            if (found == null)
            {
                found = new StateStruct(_state, def);
                states.Add(found);
            }

            // Then select either the best action (exploit) or some other (explore) 
            ActionStruct selected = null;
            
            if (rnd.NextDouble() < 0.1)
            {
                // Select random
                int inx = rnd.Next(found.actions.Count());
                selected = found.actions.ElementAt(inx);
            }
            else
            {
                // Select the best
                selected = found.actions.ElementAt(0);
                foreach (ActionStruct actstr in found.actions)
                {
                    if (actstr.expectation > selected.expectation) 
                    //if (actstr.expectation > selected.expectation || (actstr.expectation == selected.expectation && rnd.NextDouble() < 0.2)) // add some randomness if there are several similar options
                         selected = actstr;
                }
            }

            // Remember last selected actions (4 of them)
            if (last_actions.Count >= 4)
                last_actions.RemoveAt(0);
            last_actions.Add(selected);

            return selected.action;
        }

        // After action selection, we will get the immediate reward
        public void EarnReward(double _reward, int _offset)
        {
            // Depending if AI is playing one or two roles, update last or second last
            ActionStruct actstr = null;
            if (player_number == PlayerNumberEnum.AI_PLAYS_SINGLE && last_actions.Count() >= 1)
                actstr = last_actions.ElementAt(last_actions.Count() - 1);
            if (player_number == PlayerNumberEnum.AI_PLAYS_BOTH && last_actions.Count() >= _offset)
                actstr = last_actions.ElementAt(last_actions.Count() - _offset); 
            if (actstr == null)
                return;

            UpdateReward1(actstr, _reward);

            // And, if there is also the previous action, update that too (because choosing that one led to this reward)
            ActionStruct prev_actstr = null;
            if (player_number == PlayerNumberEnum.AI_PLAYS_SINGLE && last_actions.Count() >= 2)
                prev_actstr = last_actions.ElementAt(last_actions.Count() - 2);
            if (player_number == PlayerNumberEnum.AI_PLAYS_BOTH && last_actions.Count() >= (_offset + 2))
                prev_actstr = last_actions.ElementAt(last_actions.Count() - (_offset + 2));
            if (prev_actstr == null)
                return;

            // Calculate the combined reward for previous layer
/*            double reward = 0;
            int samples = 0;
            foreach (ActionStruct it in actstr.parent.actions)
            {
                if (it.samples > 0)
                {
                    reward += it.expectation;
                    samples++;
                }
            }
            if (samples > 0)
            {
                reward /= samples;
                UpdateReward1(prev_actstr, reward);
            }
*/
            double reward = 0;
            int samples = 0;
            foreach (ActionStruct it in actstr.parent.actions)
            {
                if ((samples == 0 && it.samples > 0) || (it.samples > 0 && it.expectation > reward))
                {
                    reward = it.expectation;
                    samples++;
                }
            }
            UpdateReward1(prev_actstr, reward);
        }

        private void UpdateReward1(ActionStruct _actstr, double _reward)
        {
            if (_actstr.samples == 0)
                _actstr.expectation = _reward;
            else
                _actstr.expectation = (_actstr.expectation * _actstr.samples + _reward) / (_actstr.samples + 1);
            _actstr.samples++;
        }

        private void UpdateReward2(ActionStruct _actstr, double _reward)
        {
            if (_actstr.samples == 0)
                _actstr.expectation = _reward;
            else
                _actstr.expectation = (_actstr.expectation + _reward) / 2;
            _actstr.samples++;
        }
    }
}
