namespace UB
{
    // IDLE
    public sealed class StatePlayerMovementIdle : StateMachine<PlayerMovement>
    {
        private static StatePlayerMovementIdle _instance;
        public static StatePlayerMovementIdle Instance
        {
            get
            {
                _instance ??= new StatePlayerMovementIdle();
                return _instance;
            }
        }

        public StatePlayerMovementIdle()
        {
            Init();
        }

        public override void Enter(PlayerMovement playerMovement)
        {
            playerMovement.SetCurrentMovementState(PlayerMovementState.IDLE);
            playerMovement.EnterIdleState();
            action = Update;
        }

        public override void Update(PlayerMovement playerMovement)
        {
            playerMovement.UpdateIdleState();
            //action = Exit;
        }

        public override void Exit(PlayerMovement playerMovement)
        {
            playerMovement.SetPreviousMovementState(PlayerMovementState.IDLE);
            playerMovement.ExitIdleState();
        }
    }
    
    // DEAD
    public sealed class StatePlayerMovementDead : StateMachine<PlayerMovement>
    {
        private static StatePlayerMovementDead _instance;
        public static StatePlayerMovementDead Instance
        {
            get
            {
                _instance ??= new StatePlayerMovementDead();
                return _instance;
            }
        }

        public StatePlayerMovementDead()
        {
            Init();
        }

        public override void Enter(PlayerMovement playerMovement)
        {
            playerMovement.SetCurrentMovementState(PlayerMovementState.DEAD);
            playerMovement.EnterDeadState();
            action = Update;
        }

        public override void Update(PlayerMovement playerMovement)
        {
            playerMovement.UpdateDeadState();
            //action = Exit;
        }

        public override void Exit(PlayerMovement playerMovement)
        {
            playerMovement.SetPreviousMovementState(PlayerMovementState.DEAD);
            playerMovement.ExitDeadState();
        }
    }

    // JUMP
    public sealed class StatePlayerMovementJump : StateMachine<PlayerMovement>
    {
        private static StatePlayerMovementJump _instance;
        public static StatePlayerMovementJump Instance
        {
            get
            {
                _instance ??= new StatePlayerMovementJump();
                return _instance;
            }
        }

        public StatePlayerMovementJump()
        {
            Init();
        }

        public override void Enter(PlayerMovement playerMovement)
        {
            playerMovement.SetCurrentMovementState(PlayerMovementState.JUMP);
            playerMovement.EnterJumpState();
            action = Update;
        }

        public override void Update(PlayerMovement playerMovement)
        {
            playerMovement.UpdateJumpState();
            //action = Exit;
        }

        public override void Exit(PlayerMovement playerMovement)
        {
            playerMovement.SetPreviousMovementState(PlayerMovementState.JUMP);
            playerMovement.ExitJumpState();
        }
    }

    // DOUBLEJUMP
    public sealed class StatePlayerMovementDoubleJump : StateMachine<PlayerMovement>
    {
        private static StatePlayerMovementDoubleJump _instance;
        public static StatePlayerMovementDoubleJump Instance
        {
            get
            {
                _instance ??= new StatePlayerMovementDoubleJump();
                return _instance;
            }
        }

        public StatePlayerMovementDoubleJump()
        {
            Init();
        }

        public override void Enter(PlayerMovement playerMovement)
        {
            playerMovement.SetCurrentMovementState(PlayerMovementState.DOUBLEJUMP);
            playerMovement.EnterDoubleJumpState();
            action = Update;
        }

        public override void Update(PlayerMovement playerMovement)
        {
            playerMovement.UpdateDoubleJumpState();
            //action = Exit;
        }

        public override void Exit(PlayerMovement playerMovement)
        {
            playerMovement.SetPreviousMovementState(PlayerMovementState.DOUBLEJUMP);
            playerMovement.ExitDoubleJumpState();
        }
    }

    // Fall
    public sealed class StatePlayerMovementFall : StateMachine<PlayerMovement>
    {
        private static StatePlayerMovementFall _instance;
        public static StatePlayerMovementFall Instance
        {
            get
            {
                _instance ??= new StatePlayerMovementFall();
                return _instance;
            }
        }

        public StatePlayerMovementFall()
        {
            Init();
        }

        public override void Enter(PlayerMovement playerMovement)
        {
            playerMovement.SetCurrentMovementState(PlayerMovementState.FALL);
            playerMovement.EnterFallState();
            action = Update;
        }

        public override void Update(PlayerMovement playerMovement)
        {
            playerMovement.UpdateFallState();
            //action = Exit;
        }

        public override void Exit(PlayerMovement playerMovement)
        {
            playerMovement.SetPreviousMovementState(PlayerMovementState.FALL);
            playerMovement.ExitFallState();
        }
    }

    // LEDGE
    public sealed class StatePlayerMovementLedge : StateMachine<PlayerMovement>
    {
        private static StatePlayerMovementLedge _instance;
        public static StatePlayerMovementLedge Instance
        {
            get
            {
                _instance ??= new StatePlayerMovementLedge();
                return _instance;
            }
        }

        public StatePlayerMovementLedge()
        {
            Init();
        }

        public override void Enter(PlayerMovement playerMovement)
        {
            playerMovement.SetCurrentMovementState(PlayerMovementState.LEDGE);
            playerMovement.EnterLedgeState();
            action = Update;
        }

        public override void Update(PlayerMovement playerMovement)
        {
            playerMovement.UpdateLedgeState();
            //action = Exit;
        }

        public override void Exit(PlayerMovement playerMovement)
        {
            playerMovement.SetPreviousMovementState(PlayerMovementState.LEDGE);
            playerMovement.ExitLedgeState();
        }
    }

    // GLIDING
    public sealed class StatePlayerMovementGliding : StateMachine<PlayerMovement>
    {
        private static StatePlayerMovementGliding _instance;
        public static StatePlayerMovementGliding Instance
        {
            get
            {
                _instance ??= new StatePlayerMovementGliding();
                return _instance;
            }
        }

        public StatePlayerMovementGliding()
        {
            Init();
        }

        public override void Enter(PlayerMovement playerMovement)
        {
            playerMovement.SetCurrentMovementState(PlayerMovementState.GLIDING);
            playerMovement.EnterGlidingState();
            action = Update;
        }

        public override void Update(PlayerMovement playerMovement)
        {
            playerMovement.UpdateGlidingState();
            //action = Exit;
        }

        public override void Exit(PlayerMovement playerMovement)
        {
            playerMovement.SetPreviousMovementState(PlayerMovementState.GLIDING);
            playerMovement.ExitGlidingState();
        }
    }

    // WALLJUMP
    public sealed class StatePlayerMovementWallJump : StateMachine<PlayerMovement>
    {
        private static StatePlayerMovementWallJump _instance;
        public static StatePlayerMovementWallJump Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StatePlayerMovementWallJump();
                return _instance;
            }
        }

        public StatePlayerMovementWallJump()
        {
            Init();
        }

        public override void Enter(PlayerMovement playerMovement)
        {
            playerMovement.SetCurrentMovementState(PlayerMovementState.WALLJUMP);
            playerMovement.EnterWallJumpState();
            action = Update;
        }

        public override void Update(PlayerMovement playerMovement)
        {
            playerMovement.UpdateWallJumpState();
            //action = Exit;
        }

        public override void Exit(PlayerMovement playerMovement)
        {
            playerMovement.SetPreviousMovementState(PlayerMovementState.WALLJUMP);
            playerMovement.ExitWallJumpState();
        }
    }

    // BATRUSH
    public sealed class StatePlayerMovementBatRush : StateMachine<PlayerMovement>
    {
        private static StatePlayerMovementBatRush _instance;
        public static StatePlayerMovementBatRush Instance
        {
            get
            {
                _instance ??= new StatePlayerMovementBatRush();
                return _instance;
            }
        }

        public StatePlayerMovementBatRush()
        {
            Init();
        }

        public override void Enter(PlayerMovement playerMovement)
        {
            playerMovement.SetCurrentMovementState(PlayerMovementState.BATRUSH);
            playerMovement.EnterBatRushState();
            action = Update;
        }

        public override void Update(PlayerMovement playerMovement)
        {
            playerMovement.UpdateBatRushState();
            //action = Exit;
        }

        public override void Exit(PlayerMovement playerMovement)
        {
            playerMovement.SetPreviousMovementState(PlayerMovementState.BATRUSH);
            playerMovement.ExitBatRushState();
        }
    }

    // RUSH
    public sealed class StatePlayerMovementRush : StateMachine<PlayerMovement>
    {
        private static StatePlayerMovementRush _instance;
        public static StatePlayerMovementRush Instance
        {
            get
            {
                _instance ??= new StatePlayerMovementRush();
                return _instance;
            }
        }

        public StatePlayerMovementRush()
        {
            Init();
        }

        public override void Enter(PlayerMovement playerMovement)
        {
            playerMovement.SetCurrentMovementState(PlayerMovementState.RUSH);
            playerMovement.EnterRushState();
            action = Update;
        }

        public override void Update(PlayerMovement playerMovement)
        {
            playerMovement.UpdateRushState();
            //action = Exit;
        }

        public override void Exit(PlayerMovement playerMovement)
        {
            playerMovement.SetPreviousMovementState(PlayerMovementState.RUSH);
            playerMovement.ExitRushState();
        }
    }

    // RUSH
    public sealed class StatePlayerMovementUmbrellaRush : StateMachine<PlayerMovement>
    {
        private static StatePlayerMovementUmbrellaRush _instance;
        public static StatePlayerMovementUmbrellaRush Instance
        {
            get
            {
                _instance ??= new StatePlayerMovementUmbrellaRush();
                return _instance;
            }
        }

        public StatePlayerMovementUmbrellaRush()
        {
            Init();
        }

        public override void Enter(PlayerMovement playerMovement)
        {
            playerMovement.SetCurrentMovementState(PlayerMovementState.UMBRELLARUSH);
            playerMovement.EnterUmbrellaRushState();
            action = Update;
        }

        public override void Update(PlayerMovement playerMovement)
        {
            playerMovement.UpdateUmbrellaRushState();
            //action = Exit;
        }

        public override void Exit(PlayerMovement playerMovement)
        {
            playerMovement.SetPreviousMovementState(PlayerMovementState.UMBRELLARUSH);
            playerMovement.ExitUmbrellaRushState();
        }
    }

    // SLIDING
    public sealed class StatePlayerMovementSliding : StateMachine<PlayerMovement>
    {
        private static StatePlayerMovementSliding _instance;
        public static StatePlayerMovementSliding Instance
        {
            get
            {
                _instance ??= new StatePlayerMovementSliding();
                return _instance;
            }
        }

        public StatePlayerMovementSliding()
        {
            Init();
        }

        public override void Enter(PlayerMovement playerMovement)
        {
            playerMovement.SetCurrentMovementState(PlayerMovementState.SLIDING);
            playerMovement.EnterSlidingState();
            action = Update;
        }

        public override void Update(PlayerMovement playerMovement)
        {
            playerMovement.UpdateSlidingState();
            //action = Exit;
        }

        public override void Exit(PlayerMovement playerMovement)
        {
            playerMovement.SetPreviousMovementState(PlayerMovementState.SLIDING);
            playerMovement.ExitSlidingState();
        }
    }
}