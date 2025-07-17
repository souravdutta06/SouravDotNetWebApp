pipeline {
    agent any
    options {
        skipDefaultCheckout true
        timeout(time: 30, unit: 'MINUTES')
        disableConcurrentBuilds()
    }
    environment {
        DOCKER_IMAGE = "souravdutta06/sourav-dotnet-webapp"
        DOCKER_TAG = "${env.BUILD_NUMBER}"  // Unique tag per build
        APP_VM = "sysadmin@20.57.129.245"
        APP_PORT = "80"
        HEALTH_ENDPOINT = "http://localhost:${APP_PORT}/health"
    }
    stages {
        stage('Clean Workspace') {
            steps {
                cleanWs()
            }
        }
        
        stage('Checkout SCM') {
            steps {
                checkout([
                    $class: 'GitSCM',
                    branches: scm.branches,
                    extensions: scm.extensions + [[$class: 'CloneOption', depth: 1, shallow: true]],
                    userRemoteConfigs: scm.userRemoteConfigs
                ])
            }
        }
        
        stage('Build Solution') {
            steps {
                script {
                    def buildCmd = 'dotnet build SouravDotNetWebApp.sln -c Release --no-restore'
                    if (isUnix()) {
                        sh buildCmd
                    } else {
                        bat buildCmd
                    }
                }
            }
        }
        
        stage('Test Solution') {
            steps {
                script {
                    def testCmd = 'dotnet test SouravDotNetWebApp.sln -c Release --no-build --verbosity normal'
                    if (isUnix()) {
                        sh testCmd
                    } else {
                        bat testCmd
                    }
                }
            }
        }
        
        stage('Build Docker Image') {
            steps {
                script {
                    dockerImage = docker.build("${DOCKER_IMAGE}:${DOCKER_TAG}", "--build-arg CONFIGURATION=Release .")
                }
            }
        }
        
        stage('Push to Docker Hub') {
            steps {
                withCredentials([usernamePassword(
                    credentialsId: 'docker-hub-creds', 
                    usernameVariable: 'DOCKER_USER', 
                    passwordVariable: 'DOCKER_PWD'
                )]) {
                    sh "echo \$DOCKER_PWD | docker login -u \$DOCKER_USER --password-stdin"
                    dockerImage.push()
                    dockerImage.push('latest')  // Additional latest tag
                }
            }
        }
        
        stage('Deploy to App Server') {
            steps {
                sshagent(['app-vm-ssh-key']) {
                    script {
                        // Verify SSH connection first
                        sh "ssh -o StrictHostKeyChecking=no -o BatchMode=yes ${APP_VM} 'exit'"
                        
                        sh """
                            ssh -T -o StrictHostKeyChecking=no ${APP_VM} << 'EOF'
                            # Pull the specific build version
                            docker pull ${DOCKER_IMAGE}:${DOCKER_TAG}
                            
                            # Stop and remove existing container
                            docker stop dotnet-app || true
                            docker rm dotnet-app || true
                            
                            # Start new container
                            docker run -d \\
                                --name dotnet-app \\
                                --restart=unless-stopped \\
                                -p ${APP_PORT}:80 \\
                                --health-cmd="curl -f ${HEALTH_ENDPOINT} || exit 1" \\
                                --health-interval=30s \\
                                --health-timeout=10s \\
                                --health-retries=3 \\
                                ${DOCKER_IMAGE}:${DOCKER_TAG}
                                
                            # Cleanup old images
                            docker image prune -a -f --filter "until=24h"
                            EOF
                        """
                    }
                }
            }
        }
    }
    post {
        success {
            slackSend color: 'good', message: "SUCCESS: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]'"
        }
        failure {
            slackSend color: 'danger', message: "FAILED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]'"
            sh "docker rmi ${DOCKER_IMAGE}:${DOCKER_TAG} || true"
        }
        always {
            sh 'docker logout || true'
            archiveArtifacts artifacts: '**/bin/**/*.dll,**/bin/**/*.exe', allowEmptyArchive: true
            junit '**/TestResults/**/*.xml' 
        }
    }
}
