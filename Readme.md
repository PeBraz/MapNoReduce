#MapNoReduce
Maps a function to a specific file, returing the splits. 

Created as part of the PADI course. In our implementation a client submits a job to a network of workers. A group of Job Trackers take care of scheduling tasks between all the workers.


- Not fully functional, has network concurrency problems.
- Job Trackers fail to create tasks for the entire file. 
