Curling in Virtual Reality
==========================
![curling](http://blog.ruisystem.net/wp-content/uploads/2014/12/VirtualCurling.jpg)

This is a  2-4 player Curling game that's been built atop Unity and [RUIS](http://ruisystem.net/). The game has two sterescopic projections, and one display for Oculus Rift ( broomer's view ). Stereoscopic projections view the game scene from the stone thrower's viewpoint and from the goal area.


Thrower utilizes a *Playstation Move* motion contorller to throw the stone. New game is started by pressing the *start*-button from the Move controller. Thrower spawns a new stone by pressing the *Move*-button. Player can move sideways with the *triagnle* and *square* buttons. A new throw is initiated by holding the *PS*-button and swinging the controller forwards. The speed of the swinging motion determines the speed of the throw, and the longer *PS*-button is held, the more the friction slows down the thrower's initial speed. The controller vibrates during the throw. Player can also add a spin to the stone by rotating the Move motion controller at the end of the throw. When the stone is released it's all in the hands of the broomer. Thrower can yell instructions to the broomer based on the direction and power of the throw. The thrower can also see the strength of the throw from a power meter that's rendered in one of the stereoscopic displays.


The broomer has a personal viewpoint through Oculus Rift. We used a real broom as a prop with one *Playstation Move* motion controller attached to it. The broomer can reduce the friction of the thrown stone by brooming from the central path of the stone. She can also affect the curvature of the stone by brooming from either side of the path. This motion causes the stone to alter its rotation and thus alters its path. Brooming effect is integrated over time to simulate the thin sheet of water that is formed atop ice due to brooming.


Inverse kinematics, shadows, custom physics and skybox and 3D audio were used to enhance the experience.


Authors: Chao Feng, Kaisu Ölander, Juha Rantanen, Sampo Verkasalo, and Pekka Toiminen.
