// https://velog.io/@sonohoshi/5.-%EC%83%81%ED%83%9C-%ED%8C%A8%ED%84%B4-with-Unity
// https://steadycodist.tistory.com/entry/CUnity%EB%94%94%EC%9E%90%EC%9D%B8%ED%8C%A8%ED%84%B4-%EC%83%81%ED%83%9C-%ED%8C%A8%ED%84%B4State-Pattern
namespace UB
{
    // FOLD
    public sealed class StateUmbrellaFold : StateMachine<Umbrella>
    {
        private static StateUmbrellaFold _instance;
        public static StateUmbrellaFold Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StateUmbrellaFold();
                return _instance;
            }
        }

        public StateUmbrellaFold()
        {
            Init();
        }

        public override void Enter(Umbrella umbrella)
        {
            umbrella.SetCurUmbrellaState(UmbrellaState.FOLD);
            umbrella.EnterFoldState();
            action = Update;
        }

        public override void Update(Umbrella umbrella)
        {
            umbrella.UpdateFoldState();
        }

        public override void Exit(Umbrella umbrella)
        {
            umbrella.SetPreviousUmbrellaState(UmbrellaState.FOLD);
            umbrella.ExitFoldState();
        }
    }

    // ATTACK
    public sealed class StateUmbrellaAttack : StateMachine<Umbrella>
    {
        private static StateUmbrellaAttack _instance;
        public static StateUmbrellaAttack Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StateUmbrellaAttack();
                return _instance;
            }
        }

        public StateUmbrellaAttack()
        {
            Init();
        }

        public override void Enter(Umbrella umbrella)
        {
            umbrella.SetCurUmbrellaState(UmbrellaState.ATTACK);
            umbrella.EnterAttackState();
            action = Update;
        }

        public override void Update(Umbrella umbrella)
        {
            umbrella.UpdateAttackState();
        }

        public override void Exit(Umbrella umbrella)
        {
            umbrella.SetPreviousUmbrellaState(UmbrellaState.ATTACK);
            umbrella.ExitAttackState();
        }
    }

    // GLIDING
    public sealed class StateUmbrellaGliding : StateMachine<Umbrella>
    {
        private static StateUmbrellaGliding _instance;
        public static StateUmbrellaGliding Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StateUmbrellaGliding();
                return _instance;
            }
        }

        public StateUmbrellaGliding()
        {
            Init();
        }

        public override void Enter(Umbrella umbrella)
        {
            umbrella.SetCurUmbrellaState(UmbrellaState.GLIDING);
            umbrella.EnterGlidingState();
            action = Update;
        }

        public override void Update(Umbrella umbrella)
        {
            umbrella.UpdateGlidingState();
        }

        public override void Exit(Umbrella umbrella)
        {
            umbrella.SetPreviousUmbrellaState(UmbrellaState.GLIDING);
            umbrella.ExitGlidingState();
        }
    }

    // THROW
    public sealed class StateUmbrellaThrow : StateMachine<Umbrella>
    {
        private static StateUmbrellaThrow _instance;
        public static StateUmbrellaThrow Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StateUmbrellaThrow();
                return _instance;
            }
        }

        public StateUmbrellaThrow()
        {
            Init();
        }

        public override void Enter(Umbrella umbrella)
        {
            umbrella.SetCurUmbrellaState(UmbrellaState.THROW);
            umbrella.EnterThrowState();
            action = Update;
        }

        public override void Update(Umbrella umbrella)
        {
            umbrella.UpdateThrowState();
        }

        public override void Exit(Umbrella umbrella)
        {
            umbrella.SetPreviousUmbrellaState(UmbrellaState.THROW);
            umbrella.ExitThrowState();
        }
    }

    // STICK
    public sealed class StateUmbrellaStick : StateMachine<Umbrella>
    {
        private static StateUmbrellaStick _instance;
        public static StateUmbrellaStick Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StateUmbrellaStick();
                return _instance;
            }
        }

        public StateUmbrellaStick()
        {
            Init();
        }

        public override void Enter(Umbrella umbrella)
        {
            umbrella.SetCurUmbrellaState(UmbrellaState.STICK);
            umbrella.EnterStickState();
            action = Update;
        }

        public override void Update(Umbrella umbrella)
        {
            umbrella.UpdateStickState();
        }

        public override void Exit(Umbrella umbrella)
        {
            umbrella.SetPreviousUmbrellaState(UmbrellaState.STICK);
            umbrella.ExitStickState();
        }
    }

    // RETURN
    public sealed class StateUmbrellaReturn : StateMachine<Umbrella>
    {
        private static StateUmbrellaReturn _instance;
        public static StateUmbrellaReturn Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StateUmbrellaReturn();
                return _instance;
            }
        }

        public StateUmbrellaReturn()
        {
            Init();
        }

        public override void Enter(Umbrella umbrella)
        {
            umbrella.SetCurUmbrellaState(UmbrellaState.RETURN);
            umbrella.EnterReturnState();
            action = Update;
        }

        public override void Update(Umbrella umbrella)
        {
            umbrella.UpdateReturnState();
        }

        public override void Exit(Umbrella umbrella)
        {
            umbrella.SetPreviousUmbrellaState(UmbrellaState.RETURN);
            umbrella.ExitReturnState();
        }
    }
}