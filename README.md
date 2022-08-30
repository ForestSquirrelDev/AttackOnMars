# Attack on Mars
#### A small unpolished RTS tower defense-like game prototype, made using DOTS: https://youtu.be/E-Bxr8JfyPI
## Contents
- [What is it?](#what-is-it)
- [Screenshots](#screenshots)
- [Why?](#why)
- [Gameplay](#gameplay)
- [Controls](#controls)
- [Was it worth?](#was-it-worth)

## What is it?
Attack on Mars is a simple 3D RTS (kind of) game made with Unity's data-oriented tech stack (and a bunch of stuff from Unity Asset Store 😌), meaning most things in the game is based on Unity's implementation of Entity-Component-System. The game took "inspiration" from the Series40 game "Sturmtrupp mars" where you were supposed to defend your base on Mars from upcoming bugs:

![image](https://user-images.githubusercontent.com/82777171/187526687-220afab4-8c7d-4ff5-8cea-b5ee17533fd6.png)

## Screenshots
![image](https://user-images.githubusercontent.com/82777171/185810073-7327fe83-d2f3-425b-b93f-5e93f408caec.png)
![image](https://user-images.githubusercontent.com/82777171/187528082-4cceec60-6baf-40d6-a38d-7aa46b87bff5.png)
![image](https://user-images.githubusercontent.com/82777171/187527908-a84b2f91-2caf-4786-968b-d546b1a704be.png)
![image](https://user-images.githubusercontent.com/82777171/187531536-22d1e43f-cd84-4cbc-8e96-07ba6ffb95fd.png)


## Why?
The whole idea of this project was based on following desires:
- Learn ECS concept and take a look at why it is so loved by fellow game programmers
- Touch the surface of multithreaded programming (yes, it's rather simplified in Unity DOTS by being wrapped into Jobs system, but still 🙂)
- Add Unity's Jobs system to my tools pocket
- Force myself to turn out to be in a situation where there's very little to no tutorials or even documentation on how to do things, and your only guidance is Unity's source code, their examples and tests that they write for their own engine. All this in order to become a little bit better at problem solving and researching things
- Take a look at new algorithms and come up with my own ones
- Try myself in more unfriendly environment from game engine tooling standpoint (by that i mean that when you use Unity's DOTS, you automatically give up or make your life much-much harder when working with many important, not to say crucial gamedev features such as Animations (not even Animation Curves are allowed), Audio systems, Particle systems, Pathfinding and so on), to help myself get a bit more lower-level look at game programming

## Gameplay
During the game there are enemies continuously spawned at the bunch of pre-positioned points on terrain. In order to navigate enemies, i'm using custom Flowfield pathfinding implementation with half-working local avoidance. The enemies are trying to make their way to your base. If they manage to destroy it, the game is "lost", but there's no game sequence nor any win or lose screens 😄. 
To protect your base, you have to spawn turrets, which will automatically locate enemies and shoot them. This is pretty much whole gameplay 🙂

## Controls
W/A/S/D to move the camera, mousewheel to move camera closer/further from Terrain, Right mouse click to rotate the camera in any direction, Left mouse click to place the turret on building grid.

## Was it worth?
Yes. It was pretty fun and insightful journey (or to say trip? 😁). I saw with my eyes why it sucks to have an algorithm of complexity O(n²). I loved to work with Entity-Component-System architectural pattern. I learnt a bit about Jobs system and why (or when) it can be useful. And i practically found out why they say "most often the graphics is the bottleneck in games, not the logic that is handled by CPU". 
