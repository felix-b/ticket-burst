import React, { useEffect } from 'react'
import { Box, Center, Flex, Grid, GridItem, HStack, Spacer } from "@chakra-ui/react"


const IndexPage = () => {

    const headerRef = React.useRef<HTMLDivElement>(null);

    function handleWindowScroll() {
        if (document.body.scrollTop > 80 || document.documentElement.scrollTop > 80) {
            headerRef.current.style.height = '60px';
        } else {
            headerRef.current.style.height = '80px';
        }
    }
    
    useEffect(() => {
        window.addEventListener('scroll', handleWindowScroll);
        return function cleanup() {
            window.removeEventListener('scroll', handleWindowScroll);
        };
    });
    
    return (
        <>
            <Flex as="header" 
                ref={headerRef} 
                position="fixed" 
                w="100%" 
                h="80px" 
                justifyContent={'center'}
                backgroundColor="rgba(68, 81, 90, 0.65)" 
                backdropFilter="saturate(180%) blur(5px)" 
                shadow="0px 1px 5px #677a8e" 
                style={{transition: '0.2s'}}
            >
                <Center>
                    <Flex direction={'row'} w={1200} bg="blue" justifyContent={'stretch'}>
                        <Box>
                            Here goes contents of the header....
                        </Box>
                        <Spacer />
                        <Box>
                            Here goes contents of the header....
                        </Box>
                        <Spacer />
                        <Box>
                            Here goes contents of the header....
                        </Box>
                    </Flex>
                </Center>
            </Flex>
            <Grid 
                minH="100vh"
                bg="white" 
                templateRows="500px auto"
                templateColumns="auto 1200px auto"
            >
                <GridItem  colSpan={3} bg="#a6c8b6" pt="80px">
                    <Center>
                        <Box w={1200}>
                            Here goes contents of the jumbo banner....
                        </Box>
                    </Center>
                </GridItem>
                <GridItem  h="full"></GridItem>
                <GridItem  h="full" minH={2000}>
                    <p>
                        asdfasdf
                        <br/>
                        ,bxfmbdfgdf
                        <br/>
                        ,bxfmbdfgdf
                        <br/>
                        asdfasdf
                    </p>

                </GridItem>
                <GridItem  h="full"></GridItem>
            </Grid>  
        </>
    )
}

export default IndexPage
