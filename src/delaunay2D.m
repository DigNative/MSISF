% Compile this Matlab function into a binary using Matlab and the command:
%    mcc -o delaunay2D.exe -m delaunay2D.m

function delaunay2D(csvfile)

    pattern = csvread([csvfile '.csv']);
    triang_pattern = DelaunayTri(pattern);
    dlmwrite([csvfile '_delaunay.csv'], triang_pattern, 'precision', '%li');

end