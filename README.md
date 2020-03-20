# Spoken English Words Recognition Using Neural Network

I always wanted to implement a neural network but never had time for that. This project is made only for educational purposes and will be split into a few phases of collecting the data, training the neural network, using it to control a simple game and documenting everything.

<p align="center">
  <img src="/images/neural-network.jpg" width="80%">
</p>

## Project Content

- Data Processing Tools (Node + Angular)
  - Data Collector
  
    The tool designed to collect spoken-word recordings most efficiently. You can also help me to [collect the data](https://google.com)!
  
  - Data Validator
  
    Before the data will be ready to use, it needs to be validated and processed. Only the correct recordings can be used in the neural network training process.
  
  - Data Explorer
  
    The tool allows you to easily browse the collected data. 
  
- REST API (.Net Core)

  There is one REST API that provides all needed backend functionalities. Different domains are split into different MVC controllers. The code has 100% test coverage.
  
- CI/CD Pipelines for Azure

  The project is using Microsoft Azure container instances, storage, functions, and other services. Continuous integration and deploying is provided by azure pipelines.

## Project phase 0 - Collecting the data
Training a neural network requires a lot of data. I couldn't find a good set, so I decided to collect it myself. To speed up the collecting process I made a special tool. With it, recording one set of words takes only 1 minute! You can also help me to collect the data [here](https://google.com). No recording will be captured or uploaded without your explicit action. All the collected data will be publicly available after processing.

## Project phase 1 - Neural network
To make word recognition possible, recording needs to be preprocessed in a few steps before it can be analyzed by the network.

- Finding the word start and end

  It is not always obvious where are the borders of the sound we want to analyze. In this step, the recognition algorithm will be searching for the sound intensity slopes.
  
- Getting sound frequencies

  The goal can't be achieved by analyzing only the sound intensity. To get more information about the intensity of different frequencies, the recording will be processed with the Fourier Transformation. Here is some [visual explanation](https://www.youtube.com/watch?v=spUNpyF58BY).
 
- Recording normalization

  Recordings of different voices could have various intensities and lengths. Before the recording will be turned into a graph of sound intensity in time, it needs to be "scaled" up or down in both dimensions, to fit in the frame of the neural network.

After preprocessing, the neural network can start its work. The input is the sound intensity graph. I'm not yet ready to describe properly how the neural network will be implemented and trained. One of the best basic explanations I could found is [here](https://www.youtube.com/watch?v=aircAruvnKk). This video is not answering most of the questions but is very got for the beginning. Training of the network will be done using a special training tool that also will be available in this repository. 

## Project phase 2 - Sound controlled game

To demonstrate the neural network usage in practice I decided to create a simple game based on the [Galcon Game](http://www.galcon.com/g2). Galcon is a series of real-time strategy video games. It is set in space and involves maneuvering fleets of ships to capture enemy planets. The neural network will be used only in gameplay. Navigation inside the game will be done using [Azure Cognitive Services](https://azure.microsoft.com/en-us/services/cognitive-services).

## Project phase 3 - Sharing the tools and the game

Some good description of the working neural network (if it will work) needs to be done. And after that...

<p align="center">
  <img src="/images/bane.jpg" width="50%">
</p>
