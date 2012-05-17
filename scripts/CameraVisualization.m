% NOTICE
% SPICE must be installed in Matlab prior running this script!
% Installation is easy: Just add the path to SPICE (MICE) to Matlab
% with the following two lines:
%   addpath('P:\Program Files\NASA NAIF SPICE for Matlab\src\mice');
%   addpath('P:\Program Files\NASA NAIF SPICE for Matlab\lib');


function CameraVisualization(state)

% split state
orientation = [state(5),state(6),state(7),state(8)];
pos = [state(2),state(3),state(4)];
time = state(1)+2400000.5;
alt = norm(pos) - 1.7374E6;

% load SPICE kernels
cspice_furnsh('kernels/moon_pa_de421_1900-2050.bpc');
cspice_furnsh('kernels/moon_080317.tf');
cspice_furnsh('kernels/moon_assoc_me.tf');
cspice_furnsh('kernels/pck00009.tpc');
cspice_furnsh('kernels/naif0009.tls');
cspice_furnsh('kernels/de421.bsp');

et = cspice_str2et(['JD' num2str(time)]);
[spos, lt] = cspice_spkpos('SUN', et, 'MOON_ME', 'NONE', 'MOON');

sun(1) = spos(1) * 1000;
sun(2) = spos(2) * 1000;
sun(3) = spos(3) * 1000;

cspice_kclear

% Moon radius [m]
moon_radius = 1.7374E6;
% construct a unit sphere's surface
[x,y,z] = sphere(36);

% camera direction: vector [-1,0,0] rotated by orientation quaternion
direction = qRotVec([-1,0,0], orientation);
% camera "sky" vector: vector [0,0,1] rotated by orientation quaternion
sky = qRotVec([0,0,1], orientation);

figure(1)
set(1, 'Name', 'Camera Visualization for MSISF');

% display the Moon
surf(...
    x*moon_radius,y*moon_radius,z*moon_radius,...
    'FaceColor', [.7,.7,.7]...
);
hold on

% sun position
surf(...
    (x*5E5)+(sun(1)/1E4),(y*5E5)+(sun(2)/1E4),(z*5E5)+(sun(3)/1E4),...
    'EdgeColor','none',...
    'LineStyle','none',...
    'FaceLighting','phong',...
    'FaceColor','green'...
);

% mark intercept between prime meridian and equator
[sx,sy,sz] = LatLon2xyz(moon_radius, 0, 0);
surf(...
    (x*0.5E5)+sx,(y*0.5E5)+sy,(z*0.5E5)+sz,...
    'EdgeColor','none',...
    'LineStyle','none',...
    'FaceLighting','phong',...
    'FaceColor','red'...
);

% mark north pole
[sx,sy,sz] = LatLon2xyz(moon_radius, 90, 0);
surf(...
    (x*0.5E5)+sx,(y*0.5E5)+sy,(z*0.5E5)+sz,...
    'EdgeColor','none',...
    'LineStyle','none',...
    'FaceLighting','phong',...
    'FaceColor','green'...
);

% mark (lat = 0, lon = 90°)
[sx,sy,sz] = LatLon2xyz(moon_radius, 0, 90);
surf(...
    (x*0.5E5)+sx,(y*0.5E5)+sy,(z*0.5E5)+sz,...
    'EdgeColor','none',...
    'LineStyle','none',...
    'FaceLighting','phong',...
    'FaceColor','yellow'...
);

% mark camera position
surf(...
    (x*0.5E5)+pos(1),(y*0.5E5)+pos(2),(z*0.5E5)+pos(3),...
    'EdgeColor','none',...
    'LineStyle','none',...
    'FaceLighting','phong',...
    'FaceColor','blue'...
);

% visualize camera's view direction
arrow(pos, pos + (direction*1E6),...
    'EdgeColor', 'blue', 'FaceColor', 'blue');

% visualize camera's "sky" vector
arrow(pos, pos + (sky*1E6),...
    'EdgeColor', 'red', 'FaceColor', 'red');

daspect([1 1 1])
xlabel('x');
ylabel('y');
zlabel('z');
view(113,46);
hold off

disp('#########################################################################');
disp('#           CAMERA DIRECTION AND ORIENTATION VISUALIZATION              #');
disp('#                    Matlab Demonstration Script                        #');
disp('#                 for the scene settings in the MSISF                   #');
disp('#########################################################################');
disp('#                                                                       #');
disp('# Description:                                                          #');
disp('# This script plots a visualization of the scene/camera settings for a  #');
disp('# given camera position and orientation, like the Moon Surface          #');
disp('# Illumination Software Framework (MSISF) will set-up the scene.        #');
disp('#                                                                       #');
disp('# This script is made for preview purposes only -- to test, what will   #');
disp('# be seen in the MSISF rendering using the given settings.              #');
disp('#                                                                       #');
disp('# Usage:                                                                #');
disp('#    CameraVisualization(pos, orientation)                              #');
disp('#                                                                       #');
disp('#    pos           camera position in the Moon ME/PA-frame      [m]     #');
disp('#                  (given as 1x3 matrix [x,y,z])                        #');
disp('#    orientation   camera orientation quaternion                [1]     #');
disp('#                  (given as 1x4 matrix [vx,vy,vz,r])                   #');
disp('#                                                                       #');
disp('#########################################################################');
disp('# Author:     B.Eng. René Schwarz                                       #');
disp('#             mail@rene-schwarz.com                                     #');
disp('#             http://www.rene-schwarz.com                               #');
disp('# Date:       2011/11/16                                                #');
disp('# License:    GNU General Public License (GPL), version 2 or later      #');
disp('#########################################################################');
disp(' ');disp(' ');
fmt = '%+17.15E';
fmt2 = '%12.4f';
disp('Camera position:');
disp(['    x = ' num2str(pos(1), fmt) ' m']);
disp(['    y = ' num2str(pos(2), fmt) ' m']);
disp(['    z = ' num2str(pos(3), fmt) ' m']);
disp(' ');
disp(['Camera altitude over MMR: ' num2str(alt/1E3, fmt2) ' km']);
disp(' ');
disp('Camera orientation quaternion:');
disp(['   vx = ' num2str(orientation(1), fmt)]);
disp(['   vy = ' num2str(orientation(2), fmt)]);
disp(['   vz = ' num2str(orientation(3), fmt)]);
disp([' real = ' num2str(orientation(4), fmt)]);
disp(' ');
disp('These inputs will cause the camera to look into the direction');
disp(['   dx = ' num2str(direction(1), fmt) ' m']);
disp(['   dy = ' num2str(direction(2), fmt) ' m']);
disp(['   dz = ' num2str(direction(3), fmt) ' m' ',']);
disp('whereas the camera "sky vector" will be into the direction');
disp(['   sx = ' num2str(sky(1), fmt) ' m']);
disp(['   sy = ' num2str(sky(2), fmt) ' m']);
disp(['   sz = ' num2str(sky(3), fmt) ' m' '.']);
disp(' ');
disp('A graphical visualization of the scene setting has been opened');
disp('in a separate window. Explanation of the graphical elements:');
disp(' ');
disp('   Moon:');
disp('   =====');
disp('    grey sphere: the Moon');
disp('      red point: intercept between prime meridian and equator');
disp('    green point: north pole');
disp('   yellow point: (lat = 0, lon = 90°)');
disp(' ');
disp('   Spacecraft/Camera:');
disp('   ==================');
disp('     blue point: the spacecraft/camera');
disp('     blue arrow: camera pointing (a.k.a. "camera optical axis")');
disp('      red arrow: camera "sky vector"');
disp(' ');
disp('REMARKS:');
disp('Without giving a camera orientation (quaternion [0,0,0,1]), the camera');
disp('will look into the direction [-1,0,0] from it''s actual position. The');
disp('sky vector is set to [0,0,1]. If a orientation quaternion not equal to');
disp('[0,0,0,1] is given, these two vectors will be rotated using the given');
disp('orientation quaternion.');
disp('All spatial positions/vectors/directions are written in the Moon''s');
disp('ME/PA frame, as defined by the NASA (adopted by the IAU).');

end

function v = qRotVec(vec, q)
% returns vector v, which is calculated through the rotation of vec with
% quaternion q.

qVec = [vec, 0];
qCon = qConj(q);

qV = qMult(qMult(q, qVec), qCon);
v = [qV(1), qV(2), qV(3)];

end

function q = qMult(q1, q2)
% returns quaternion product for the two given quaternions q1 and q2
% Notation:
%  Every quaternion is written as a 1x4 matrix:
%      q = [vx, vy, vz, r]

q = [q1(4)*q2(1) + q1(1)*q2(4) + q1(2)*q2(3) - q1(3)*q2(2),...
     q1(4)*q2(2) - q1(1)*q2(3) + q1(2)*q2(4) + q1(3)*q2(1),...
     q1(4)*q2(3) + q1(1)*q2(2) - q1(2)*q2(1) + q1(3)*q2(4),...
     q1(4)*q2(4) - q1(1)*q2(1) - q1(2)*q2(2) - q1(3)*q2(3)];
end

function qConj = qConj(q)

qConj = [-q(1), -q(2), -q(3), q(4)];

end

function [x,y,z] = LatLon2xyz(r, lat, lon)

deg2rad = (pi/180);
x = r * cos(lat*deg2rad) * cos(lon*deg2rad);
y = r * cos(lat*deg2rad) * sin(lon*deg2rad);
z = r * sin(lat*deg2rad);

end